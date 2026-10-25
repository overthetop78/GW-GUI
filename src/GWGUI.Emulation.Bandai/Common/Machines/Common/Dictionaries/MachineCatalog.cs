namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = Array.AsReadOnly(
        ModelCatalog.All.Select(model => new EmulationMachineDefinition(model.Id, model.DisplayResourceKey, $"{EmulationModuleConstants.AssetResourcePrefix}.Machines.{model.Id}.png")).ToArray());
}
