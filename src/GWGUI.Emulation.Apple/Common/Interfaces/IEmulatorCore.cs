using GWGUI.Emulation.Interop;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Apple.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>
{

    Emulator Emulator { get; }
}
