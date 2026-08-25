using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Dictionaries;

public static class AmigaMachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = AmigaModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id, $"Emulation.Amiga.Model.{model.Id}"))
        .ToArray();
}
