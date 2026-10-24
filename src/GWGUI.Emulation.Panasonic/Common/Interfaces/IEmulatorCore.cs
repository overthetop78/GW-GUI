using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Panasonic.Common.Contracts;
using GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Panasonic.Common.Interfaces;

internal interface IEmulatorCore : IEmulationCoreRuntime<MachineConfiguration, CoreOption>, IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>, IEmulationCoreIdentity<Emulator>
{
}
