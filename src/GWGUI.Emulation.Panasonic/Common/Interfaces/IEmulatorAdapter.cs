using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Panasonic.Common.Contracts;
using GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Panasonic.Common.Services;
namespace GWGUI.Emulation.Panasonic.Common.Interfaces;
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>
{
    IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration);
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
