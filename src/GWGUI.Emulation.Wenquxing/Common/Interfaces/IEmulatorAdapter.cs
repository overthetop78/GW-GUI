using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Wenquxing.Common.Contracts;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Wenquxing.Common.Services;
namespace GWGUI.Emulation.Wenquxing.Common.Interfaces;
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>
{
    IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration);
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
