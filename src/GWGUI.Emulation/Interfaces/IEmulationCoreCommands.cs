using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Interfaces;

internal interface IEmulationCoreCommands<TMedia, TPeripheral, TDiskStatus>
{
    void HardReset();
    void Stop();
    void InsertMedia(TMedia media);
    void EjectMedia(EmulationMediaSlot slot);
    byte[] SaveState();
    void LoadState(ReadOnlySpan<byte> state);
    void SetOption(string key, string value);
    void SelectDisk(int index);
    TDiskStatus GetDiskStatus();
    void SetControllerPortDevice(int port, TPeripheral peripheral);
}
