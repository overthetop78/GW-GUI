using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = ModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id, $"Emulation.Amiga.Model.{model.Id}"))
        .ToArray();
}
