using GWGUI.Emulation;
using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Apple.Common.Machines.Common.Dictionaries;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        return new EmulationConfigurationSummary(hardware.DisplayNameResourceKey, [$"Core {configuration.Core}"]);
    }
}
