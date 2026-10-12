using System.IO;

namespace GWGUI.Emulation.Philips.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private void PrepareInitialMedia(MachineConfiguration configuration)
    {
        var media = configuration.Media.SingleOrDefault(value => value.IsInserted);
        if (media is null) return;
        var source = ContentFunctions.Validate(media.Path, _info.Extensions);
        var size = new FileInfo(source).Length;
        if (size == 0) throw InvalidContent();
        if (configuration.Core == Emulator.O2em && !IsValidVideopacCartSize(size))
            throw InvalidContent();
        // M2000 opens supplied content read-only; only its no-content Default.cas is writable.
        if (configuration.Core == Emulator.M2000 && !media.IsReadOnly)
            throw InvalidContent();
        _content = ContentFunctions.Create(source, _info.NeedsFullPath, _info.Extensions);
    }

    private static bool IsValidVideopacCartSize(long size)
    {
        if (MediaRuntimeConstants.MegaCartSizes.Contains(size)) return true;
        var unit = size % MediaRuntimeConstants.VideopacLargeBankBytes == 0 ? MediaRuntimeConstants.VideopacLargeBankBytes : MediaRuntimeConstants.VideopacSmallBankBytes;
        return size >= unit && size % unit == 0 && size / unit <= MediaRuntimeConstants.VideopacRomBanks;
    }

    public void InsertMedia(MediaConfiguration media)
    {
        var configuration = RequireConfiguration();
        var next = configuration with
        {
            Media = configuration.Media.Where(item => item.Slot != media.Slot).Append(media with { IsInserted = true }).ToArray()
        };
        ConfigurationFunctions.Validate(next);
        if (Emulator != Emulator.SameCdi || media.Slot != EmulationMediaSlot.Cd0
            || string.Equals(Path.GetExtension(media.Path), MediaRuntimeConstants.PlaylistExtension, StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
        var path = ContentFunctions.Validate(media.Path, _info.Extensions);
        RequireCallbacks().DiskControl.Insert(path);
        _configuration = next;
    }

    public void EjectMedia(EmulationMediaSlot slot)
    {
        var configuration = RequireConfiguration();
        if (Emulator != Emulator.SameCdi || slot != EmulationMediaSlot.Cd0)
            throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
        RequireCallbacks().DiskControl.Eject();
        _configuration = configuration with
        {
            Media = configuration.Media.Where(item => item.Slot != slot).ToArray()
        };
    }

    public void SelectDisk(int index) => RequireCallbacks().DiskControl.Select(index);
    public DiskStatus GetDiskStatus() => RequireCallbacks().DiskControl.GetStatus();
    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        if (!ConfigurationFunctions.DefaultSlots(RequireConfiguration().Model).Contains(slot))
            throw new ArgumentOutOfRangeException(nameof(slot));
        // Loaded media is read-only. Native cassette/NVRAM writes remain in the persistent save directory.
    }
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;
    private MachineConfiguration RequireConfiguration() => _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
    private static EmulationException InvalidContent() => new(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
}
