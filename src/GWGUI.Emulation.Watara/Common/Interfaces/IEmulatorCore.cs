using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Watara.Common.Contracts;
using GWGUI.Emulation.Watara.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Watara.Common.Machines.Common.Enums;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Watara.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>
{

    Emulator Emulator { get; }
}
