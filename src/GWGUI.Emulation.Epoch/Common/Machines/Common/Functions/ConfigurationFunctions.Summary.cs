using CommonCoreHostFunctions = GWGUI.Emulation.Epoch.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration) => new(CommonCoreHostFunctions.Adapters.DisplayResourceKey(ModelCatalog.All, configuration), [EmulatorCatalog.Get(configuration.Core).DisplayName]);
}

