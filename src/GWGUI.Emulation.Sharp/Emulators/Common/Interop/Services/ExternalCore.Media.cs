using GWGUI.Emulation.Sharp.Emulators.Common.Dictionaries;
using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;
using System.IO;
namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private void PrepareInitialMedia(MachineConfiguration configuration) => _content = ((LibretroMachineFactory)CoreCatalog.CreateAdapter(Emulator)).PrepareContent(configuration, _sessionDirectory ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized), _info.Extensions);
    public void InsertMedia(MediaConfiguration media)
    {
        var configuration = RequireConfiguration();
        var next = configuration with { Media = configuration.Media.Where(item => item.Slot != media.Slot).Append(media with { IsInserted = true }).ToArray() };
        ConfigurationFunctions.Validate(next);
        var control = RequireCallbacks().DiskControl;
        if (media.Slot != EmulationMediaSlot.Floppy0 || media.Category != MediaCategory.Floppy || !control.IsAvailable)
            throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
        var device = ((LibretroMachineFactory)CoreCatalog.CreateAdapter(Emulator)).DescribeMediaDevices(configuration).Single(device => device.Slot == media.Slot);
        // A fresh session path avoids overwriting the image still open in the native drive.
        var directory = Path.Combine(_sessionDirectory ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized), Guid.NewGuid().ToString("N"));
        var path = SessionMediaFunctions.Prepare(media, directory, device.AcceptedExtensions);
        control.Insert(path);
        _configuration = next;
    }

    public void EjectMedia(EmulationMediaSlot slot)
    {
        var configuration = RequireConfiguration();
        var control = RequireCallbacks().DiskControl;
        if (slot != EmulationMediaSlot.Floppy0 || !control.IsAvailable) throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
        control.Eject();
        _configuration = configuration with { Media = configuration.Media.Where(item => item.Slot != slot).ToArray() };
    }
    public void SelectDisk(int index) => RequireCallbacks().DiskControl.Select(index);
    public DiskStatus GetDiskStatus() => RequireCallbacks().DiskControl.GetStatus();
    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        if (!RequireConfiguration().ConfiguredSlots.Contains(slot)) throw new ArgumentOutOfRangeException(nameof(slot));
    }
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;
    private MachineConfiguration RequireConfiguration() => _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
}
