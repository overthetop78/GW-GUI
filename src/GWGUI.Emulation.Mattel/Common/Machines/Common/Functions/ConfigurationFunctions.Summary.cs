using CommonCoreHostFunctions = GWGUI.Emulation.Mattel.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration) => new(CommonCoreHostFunctions.Adapters.DisplayResourceKey(ModelCatalog.All, configuration), [new Engine().Adapter(configuration).Definition.DisplayName]);
}
