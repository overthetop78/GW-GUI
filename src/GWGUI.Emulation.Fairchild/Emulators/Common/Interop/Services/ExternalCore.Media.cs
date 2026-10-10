using System.IO;

namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private void PrepareInitialMedia(MachineConfiguration configuration)
    {
        var cartridge = configuration.Media.SingleOrDefault(value => value.IsInserted);
        if (cartridge is null) return;
        var source = ContentFunctions.Validate(cartridge.Path, _info.Extensions);
        if (new FileInfo(source).Length == 0)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
        _content = ContentFunctions.Create(source, _info.NeedsFullPath, _info.Extensions);
    }

    public void InsertMedia(MediaConfiguration media) => throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
    public void EjectMedia(EmulationMediaSlot slot)
    {
        if (slot != EmulationMediaSlot.Cartridge0) throw new ArgumentOutOfRangeException(nameof(slot));
        Stop();
    }
    // The machine owner recreates the isolated process for cart insertion/replacement.
    // FreeChaF unload/deinit do not release the cart allocation; never reload this instance.
    public void SelectDisk(int index) => throw new ArgumentOutOfRangeException(nameof(index));
    public DiskStatus GetDiskStatus() => RequireCallbacks().DiskControl.GetStatus();
    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        if (slot != EmulationMediaSlot.Cartridge0) throw new ArgumentOutOfRangeException(nameof(slot));
    }
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;
}
