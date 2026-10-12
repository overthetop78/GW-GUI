using GWGUI.Emulation.Thomson.Common.Dictionaries;

namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration) => new(ModelCatalog.All.Single(model => model.Id == configuration.MachineId).DisplayResourceKey, configuration.Core == Emulator.None ? [] : [EmulatorCatalog.Get(configuration.Core).DisplayName]);
}
