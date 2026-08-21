namespace GWGUI.App.Controls;

public sealed class EmulationStorageDeviceEventArgs(EmulationStorageDeviceItem device) : EventArgs
{
    public EmulationStorageDeviceItem Device { get; } = device;
}
