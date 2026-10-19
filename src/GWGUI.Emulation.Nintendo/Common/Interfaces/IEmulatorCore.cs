namespace GWGUI.Emulation.Nintendo.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCorePathMediaCommands
{
    void SoftReset();
}
