namespace GWGUI.Emulation.Snk.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = Array.AsReadOnly(ModelCatalog.All.Select(model => new EmulationMachineDefinition(model.Id, model.DisplayResourceKey, $"{EmulationModuleConstants.AssetPathPrefix}/Machines/{ModelCatalog.Parse(model.Id)}.png")).ToArray());
}
