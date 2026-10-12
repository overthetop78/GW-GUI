using GWGUI.Emulation.Watara.Common.Dictionaries;

namespace GWGUI.Emulation.Watara.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration) => new(ModelCatalog.All.Single(model => model.Id == configuration.MachineId).DisplayResourceKey, configuration.Core == Emulator.None ? [] : [EmulatorCatalog.Get(configuration.Core).DisplayName]);
}
