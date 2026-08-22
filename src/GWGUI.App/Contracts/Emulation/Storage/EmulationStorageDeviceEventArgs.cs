namespace GWGUI.App.Contracts.Emulation.Storage;

public sealed class EmulationStorageDeviceEventArgs(EmulationStorageDeviceItem device) : EventArgs
{
    public EmulationStorageDeviceItem Device { get; } = device;
}
