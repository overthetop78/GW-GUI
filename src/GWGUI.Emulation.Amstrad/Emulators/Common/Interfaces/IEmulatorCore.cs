using GWGUI.Emulation;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreStateCommands
{
    bool SupportsPointerCapture => true;
    int DiskCount { get; }
    int CurrentDiskIndex { get; }
    void SoftReset();
    void InsertMedia(string path);
    void EjectMedia();
}
