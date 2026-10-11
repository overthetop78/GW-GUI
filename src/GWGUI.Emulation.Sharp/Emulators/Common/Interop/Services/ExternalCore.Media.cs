namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private void PrepareInitialMedia(MachineConfiguration configuration) => _content = ((LibretroMachineFactory)CoreCatalog.CreateAdapter(Emulator)).PrepareContent(configuration, _sessionDirectory ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized), _info.Extensions);
    public void InsertMedia(MediaConfiguration media) => throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
    public void EjectMedia(EmulationMediaSlot slot) => throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
    public void SelectDisk(int index) => RequireCallbacks().DiskControl.Select(index);
    public DiskStatus GetDiskStatus() => RequireCallbacks().DiskControl.GetStatus();
    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        if (!RequireConfiguration().ConfiguredSlots.Contains(slot)) throw new ArgumentOutOfRangeException(nameof(slot));
    }
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;
    private MachineConfiguration RequireConfiguration() => _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
}
