using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Thomson.Common.Contracts;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Thomson.Common.Services;
namespace GWGUI.Emulation.Thomson.Common.Interfaces;
internal interface IEmulatorAdapter : global::GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>
{
    IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration);
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
