using GWGUI.Emulation.Apple.Emulators.Common.Constants;
using GWGUI.Emulation.Apple.Emulators.Common.Contracts;
using GWGUI.Emulation.Apple.Emulators.Common.Exceptions;
using GWGUI.Emulation.Apple.Emulators.Common.Functions;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Interop;
using System.IO;
using AppleWinOptions = GWGUI.Emulation.Apple.Emulators.AppleWin.Constants.OptionConstants;

namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private SessionMedia? _initialMedia;

    private void PrepareInitialMedia(MachineConfiguration configuration)
    {
        foreach (var media in configuration.Media.Where(item => item.IsInserted))
            _sessionMedia.Add(SessionMediaFunctions.Prepare(media, _sessionDirectory!, _info.Extensions));
        _initialMedia = _sessionMedia.FirstOrDefault(item => item.Configuration.Category == MediaCategory.HardDisk) ?? _sessionMedia.FirstOrDefault(item => item.Configuration.Slot == EmulationMediaSlot.Floppy0);
        if (Emulator == Emulator.MiniVmac) _initialMedia = _sessionMedia.FirstOrDefault();
        if (_initialMedia is not null)
            _content = ContentFunctions.Create(_initialMedia.RuntimePath, _info.NeedsFullPath, _info.Extensions);
    }

    private void AttachAdditionalMedia()
    {
        if (Emulator != Emulator.AppleWin) return;
        foreach (var media in _sessionMedia.Where(item => !ReferenceEquals(item, _initialMedia)))
        {
            if (media.Configuration.Category != MediaCategory.Floppy ||
                string.Equals(Path.GetExtension(media.RuntimePath), SessionMediaConstants.PlaylistExtension, StringComparison.OrdinalIgnoreCase))
                throw DynamicMediaUnavailable();
            WithDrive(media.Configuration.Slot, () => RequireCallbacks().DiskControl.InsertAt(
                media.Configuration.Slot == EmulationMediaSlot.Floppy1 ? SessionMediaConstants.SecondMediaIndex : SessionMediaConstants.FirstMediaIndex, media.RuntimePath));
        }
    }

    public void InsertMedia(MediaConfiguration media)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        ConfigurationFunctions.Validate(configuration with { Media = [media] });
        if (Emulator != Emulator.AppleWin || media.Category != MediaCategory.Floppy ||
            string.Equals(Path.GetExtension(media.Path), SessionMediaConstants.PlaylistExtension, StringComparison.OrdinalIgnoreCase))
            throw DynamicMediaUnavailable();
        var prepared = SessionMediaFunctions.Prepare(media, _sessionDirectory!, _info.Extensions);
        WithDrive(media.Slot, () => RequireCallbacks().DiskControl.InsertAt(media.Slot == EmulationMediaSlot.Floppy1 ? SessionMediaConstants.SecondMediaIndex : SessionMediaConstants.FirstMediaIndex, prepared.RuntimePath));
        _sessionMedia.Add(prepared);
    }

    public void EjectMedia(EmulationMediaSlot slot)
    {
        if (Emulator != Emulator.AppleWin ||
            (slot != EmulationMediaSlot.Floppy0 && slot != EmulationMediaSlot.Floppy1))
            throw DynamicMediaUnavailable();
        WithDrive(slot, () => RequireCallbacks().DiskControl.Eject());
    }

    private void WithDrive(EmulationMediaSlot slot, Action action)
    {
        var callbacks = RequireCallbacks();
        var previous = callbacks.OptionDocumentValues.GetValueOrDefault(AppleWinOptions.DiskControlDrive, AppleWinOptions.Drive1);
        try
        {
            callbacks.SetOption(AppleWinOptions.DiskControlDrive, slot == EmulationMediaSlot.Floppy1 ? AppleWinOptions.Drive2 : AppleWinOptions.Drive1);
            action();
        }
        finally
        {
            callbacks.SetOption(AppleWinOptions.DiskControlDrive, previous);
        }
    }

    public void SelectDisk(int index) => RequireCallbacks().DiskControl.Select(index);
    public DiskStatus GetDiskStatus() => RequireCallbacks().DiskControl.GetStatus();

    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        var media = _sessionMedia.LastOrDefault(item => item.Configuration.Slot == slot) ?? throw new InvalidOperationException(SessionMediaErrors.ExplicitSaveRequired);
        SessionMediaFunctions.Save(media);
    }

    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => _sessionMedia.LastOrDefault(item => item.Configuration.Slot == slot)?.RequiresExplicitSave == true;

    private static EmulationException DynamicMediaUnavailable() => new(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.DynamicMediaUnsupported);
}
