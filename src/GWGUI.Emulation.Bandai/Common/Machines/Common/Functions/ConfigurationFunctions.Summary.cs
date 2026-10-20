namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration) => new(GWGUI.Emulation.Functions.EmulationMachineModelFunctions.DisplayResourceKey(ModelCatalog.All, configuration), [EmulatorCatalog.GetDefinition(configuration.Core).DisplayName]);
}
