using System.IO;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCorePathMediaCommands
{
}
