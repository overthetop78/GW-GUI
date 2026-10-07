using GWGUI.Emulation;
using GWGUI.Emulation.Commodore.Common.Constants;

namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = ModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id,
            model.Id == "CDTV" ? "Emulation.Commodore.Model.CDTV" : $"Emulation.Amiga.Model.{model.Id}",
            $"{EmulationModuleConstants.AssetResourcePrefix}.Machines.{model.Id}.png"))
        .ToArray();
}
