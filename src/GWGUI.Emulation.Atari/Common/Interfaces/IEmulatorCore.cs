using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Interfaces;

internal interface IEmulatorCore : IEmulationCoreRuntime<MachineConfiguration, CoreOption>, IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>, IEmulationCoreIdentity<Emulator>
{
}
