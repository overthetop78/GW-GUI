using GWGUI.Emulation;
using GWGUI.Emulation.Amstrad.Common.Constants;

namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = ModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id,
            MachineConfigurationConstants.ResourcePrefix + model.Id,
            $"{EmulationModuleConstants.AssetResourcePrefix}.Machines.{ImageFileName(model.Id)}"))
        .ToArray();

    private static string ImageFileName(string machineId) => machineId switch
    {
        "cpc-464" => "CPC464.png",
        "cpc-664" => "CPC664.png",
        "cpc-6128" => "CPC6128.png",
        "cpc-464-plus" => "CPC464PLUS.png",
        "cpc-6128-plus" => "CPC6128PLUS.png",
        "gx4000" => "GX4000.png",
        _ => throw new ArgumentOutOfRangeException(nameof(machineId), machineId, null)
    };
}
