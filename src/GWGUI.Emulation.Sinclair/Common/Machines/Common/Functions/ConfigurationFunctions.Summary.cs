namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration) => new(ModelCatalog.All.Single(model => model.Id == configuration.MachineId).DisplayResourceKey, configuration.Core == Emulator.None ? [] : [new Engine().Adapter(configuration).Definition.DisplayName]);
}
