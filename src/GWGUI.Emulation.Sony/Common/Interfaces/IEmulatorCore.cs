namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCorePathMediaCommands
{
    bool ShutdownRequested => false;
    void SoftReset();
    void SetControllerPortDevice(int port, PeripheralCategory type);
}
