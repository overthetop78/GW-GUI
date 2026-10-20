using GWGUI.Emulation.Watara.Common.Dictionaries;

namespace GWGUI.Emulation.Watara.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration) => new(GWGUI.Emulation.Functions.EmulationMachineModelFunctions.DisplayResourceKey(ModelCatalog.All, configuration), configuration.Core == Emulator.None ? [] : [EmulatorCatalog.Get(configuration.Core).DisplayName]);
}
