namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration,
        IReadOnlyList<CoreOption>? coreOptions = null) =>
        new Engine().Adapter(configuration).GetSettingsBlocks(configuration, coreOptions ?? []);
}
