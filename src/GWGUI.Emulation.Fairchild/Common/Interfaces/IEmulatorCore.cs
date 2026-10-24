using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Fairchild.Common.Interfaces;

internal interface IEmulatorCore : IEmulationCoreRuntime<MachineConfiguration, CoreOption>, IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>, IEmulationCoreIdentity<Emulator>
{
}
