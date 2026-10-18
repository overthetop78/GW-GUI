using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Interfaces;

internal interface IEmulationCoreCommands<TMedia, TPeripheral, TDiskStatus> : IEmulationCoreStateCommands
{
    void InsertMedia(TMedia media);
    void EjectMedia(EmulationMediaSlot slot);
    TDiskStatus GetDiskStatus();
    void SetControllerPortDevice(int port, TPeripheral peripheral);
}
