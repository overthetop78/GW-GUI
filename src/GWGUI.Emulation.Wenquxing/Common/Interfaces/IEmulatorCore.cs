using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Wenquxing.Common.Contracts;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Wenquxing.Common.Interfaces;

internal interface IEmulatorCore : IEmulationCoreRuntime<MachineConfiguration, CoreOption>, IEmulationCoreCommands<MediaConfiguration, EmulationPeripheralCategory>, IEmulationCoreIdentity<Emulator>
{
}
