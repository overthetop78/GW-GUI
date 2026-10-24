using GWGUI.Emulation.Interfaces;
namespace GWGUI.Emulation.Nintendo.Common.Interfaces;

internal interface IEmulatorCore : IEmulationCoreRuntime<MachineConfiguration, CoreOption>, IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>, IEmulationCoreIdentity<string>
{
}
