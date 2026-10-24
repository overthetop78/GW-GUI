using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Apple.Common.Interfaces;

internal interface IEmulatorCore : IEmulationCoreRuntime<MachineConfiguration, CoreOption>, IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>, IEmulationCoreIdentity<Emulator>
{
}
