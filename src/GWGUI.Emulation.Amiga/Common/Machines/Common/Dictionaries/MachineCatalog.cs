using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Common.Constants;

namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = ModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id,
            $"Emulation.Amiga.Model.{model.Id}",
            $"{EmulationModuleConstants.AssetResourcePrefix}.Machines.{model.Id}.png"))
        .ToArray();
}
