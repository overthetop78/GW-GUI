using GWGUI.Emulation;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = ModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id, model.DisplayResourceKey,
            ModelCatalog.Parse(model.Id) != MachineModel.BbcMaster128 ? EmulationModuleConstants.BbcMicroImageResourceName : $"{EmulationModuleConstants.AssetResourcePrefix}.Machines.{model.Id}.png")).ToArray();
}
