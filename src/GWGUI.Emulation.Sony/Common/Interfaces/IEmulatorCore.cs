namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreStateCommands
{
    bool ShutdownRequested => false;
    IReadOnlyList<ControllerFeedbackState> ControllerFeedback => [];
    int DiskCount { get; }
    int CurrentDiskIndex { get; }
    void SoftReset();
    void SetControllerPortDevice(int port, ControllerType type);
    void InsertMedia(string path);
    void EjectMedia();
}
