using System.IO;
using System.Security.Cryptography;
using GearOptions = GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants.OptionConstants;
using Subsystem = GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants.SubsystemConstants;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private void PrepareInitialMedia(MachineConfiguration configuration)
    {
        var media = configuration.Media.Where(value => value.IsInserted).ToArray();
        var cartridge = media.SingleOrDefault(value => value.Category == MediaCategory.Cartridge);
        if (cartridge is not null)
        {
            var path = ContentFunctions.Validate(cartridge.Path, _info.Extensions);
            _batterySavePath = Path.Combine(_saveDirectory!, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant() + MediaConstants.BatterySaveExtension);
        }
        if (configuration.Model == MachineModel.Adam)
        {
            var paths = new string?[Subsystem.ContentCount];
            foreach (var item in media)
            {
                var index = item.Category switch
                {
                    MediaCategory.Cartridge => Subsystem.Cartridge,
                    MediaCategory.Floppy => Subsystem.Disk1 + item.Slot.Index,
                    MediaCategory.DataPack => Subsystem.DataPack1 + item.Slot.Index,
                    _ => throw new ArgumentOutOfRangeException(nameof(configuration))
                };
                paths[index] = item.Path;
            }
            _subsystem = LoadedSubsystem.Create(paths, _info.NeedsFullPath, _info.Extensions);
        }
        else if (cartridge is not null)
            _content = ContentFunctions.Create(cartridge.Path, _info.NeedsFullPath, _info.Extensions);
    }

    public void InsertMedia(MediaConfiguration media)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        if (!ConfigurationFunctions.SupportsSlot(configuration.Model, media.Slot))
            throw new ArgumentOutOfRangeException(nameof(media));
        ContentFunctions.Validate(media.Path, _info.Extensions);
        var replacement = configuration.Media.Where(value => value.Slot != media.Slot).Append(media with { IsInserted = true }).ToArray();
        var next = configuration with { Media = replacement };
        ConfigurationFunctions.Validate(next);
        var mounted = replacement.Where(value => value.IsInserted && value.Category != MediaCategory.Cartridge).ToArray();
        if (mounted.Select(value => value.IsReadOnly).Distinct().Count() > 1)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
        var wasWritable = configuration.Media.Any(value => value.IsInserted && value.Category != MediaCategory.Cartridge && !value.IsReadOnly);
        var isWritable = mounted.Any(value => !value.IsReadOnly);
        if (media.Category == MediaCategory.Cartridge || wasWritable != isWritable)
            ReloadMedia(next);
        else
        {
            SelectDrive(media.Slot);
            RequireCallbacks().DiskControl.Insert(media.Path);
            _configuration = next;
        }
    }

    public void EjectMedia(EmulationMediaSlot slot)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        var next = configuration with { Media = configuration.Media.Where(value => value.Slot != slot).ToArray() };
        if (slot.Category == EmulationMediaCategory.CartridgeSlot) ReloadMedia(next);
        else
        {
            SelectDrive(slot);
            RequireCallbacks().DiskControl.Eject();
            _configuration = next;
        }
    }

    private void ReloadMedia(MachineConfiguration next)
    {
        var session = _sessionDirectory!;
        var saves = _saveDirectory;
        DisposeNativeResources();
        Initialize(next, session, saves);
    }

    private void SelectDrive(EmulationMediaSlot slot)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        if (configuration.Model != MachineModel.Adam || !ConfigurationFunctions.SupportsSlot(configuration.Model, slot)
            || slot.Category == EmulationMediaCategory.CartridgeSlot)
            throw new ArgumentOutOfRangeException(nameof(slot));
        var index = slot.Index + (slot.Category == EmulationMediaCategory.CassetteDrive ? StorageConstants.DriveCount : 0);
        RequireCallbacks().SetOption(GearOptions.OptionAdamDiskDrive, MediaRuntimeConstants.DriveNames[index]);
        // Gearcoleco consumes this option in retro_run before selecting the disk-control target.
        RequireExports().Run();
    }

    public void SelectDisk(int index) => RequireCallbacks().DiskControl.Select(index);
    public DiskStatus GetDiskStatus() => RequireCallbacks().DiskControl.GetStatus();

    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        if (slot.Category == EmulationMediaCategory.CartridgeSlot)
        {
            WriteBatteryMemory(RequireExports());
            return;
        }
        SelectDrive(slot);
        var disk = RequireCallbacks().DiskControl;
        var status = disk.GetStatus();
        if (status.IsEjected) return;
        // Ejection flushes Gearcoleco's save-directory working copy, without overwriting the source.
        disk.Eject();
        disk.Select(status.CurrentIndex);
    }

    // Native writable media is persisted by the core; no native dirty-state export exists.
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;
}
