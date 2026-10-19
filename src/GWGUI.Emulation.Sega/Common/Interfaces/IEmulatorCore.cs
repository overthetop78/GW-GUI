using GWGUI.Emulation;

namespace GWGUI.Emulation.Sega.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCorePathMediaCommands
{
    void SoftReset();
}
