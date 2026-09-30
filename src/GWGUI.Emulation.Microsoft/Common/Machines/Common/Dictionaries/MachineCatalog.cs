using GWGUI.Emulation;
using GWGUI.Emulation.Microsoft.Common.Constants;

namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = ModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id,
            MachineConfigurationConstants.ResourcePrefix + model.Id,
            $"{EmulationModuleConstants.AssetResourcePrefix}.Machines.{model.Id}.png"))
        .ToArray();
}
