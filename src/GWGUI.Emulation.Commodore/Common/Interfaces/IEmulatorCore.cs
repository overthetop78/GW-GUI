using GWGUI.Emulation.Interfaces;
using System.IO;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Interfaces;

internal interface IEmulatorCore : IEmulationCoreRuntime<MachineConfiguration, CoreOption>, IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>, IEmulationCoreIdentity<string>
{
}
