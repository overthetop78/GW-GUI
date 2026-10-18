namespace GWGUI.Emulation.Nintendo.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreStateCommands
{
    int DiskCount { get; }
    int CurrentDiskIndex { get; }
    void SoftReset();
    void InsertMedia(string path);
    void EjectMedia();
}
