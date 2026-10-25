using CommonCoreHostFunctions = GWGUI.Emulation.Sharp.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration) => new(CommonCoreHostFunctions.Adapters.DisplayResourceKey(ModelCatalog.All, configuration), configuration.Core == Emulator.None ? [] : [new Engine().Adapter(configuration).Definition.DisplayName]);
}
