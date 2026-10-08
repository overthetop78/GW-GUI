namespace GWGUI.Emulation.Nec.Common.Machines.Common.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) =>
        new Engine().Adapter(configuration).GetSettingsBlocks(configuration);
}
