namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration) => new(ModelCatalog.All.Single(model => model.Id == configuration.MachineId).DisplayResourceKey, [new Engine().Adapter(configuration).Definition.DisplayName]);
}
