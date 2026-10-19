using GWGUI.Emulation;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCorePathMediaCommands
{
    bool SupportsPointerCapture => true;
    void SoftReset();
}
