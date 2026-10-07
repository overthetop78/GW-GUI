using GWGUI.Emulation.Amstrad.Common.Machines.Gx4000.Constants;
using GWGUI.Emulation.Amstrad.Common.Machines.CpcPlus.Constants;
using GWGUI.Emulation.Amstrad.Common.Machines.CpcClassic.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = ModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id,
            MachineConfigurationConstants.ResourcePrefix + model.Id,
            $"{EmulationModuleConstants.AssetResourcePrefix}{MachineCatalogConstants.MachineNamespaceMarker}{ImageFileName(model.Id)}"))
        .ToArray();

    private static string ImageFileName(string machineId) => machineId switch
    {
        global::GWGUI.Emulation.Amstrad.Common.Machines.CpcClassic.Constants.ModelConstants.Cpc464 => MachineCatalogConstants.CPC464Png,
        global::GWGUI.Emulation.Amstrad.Common.Machines.CpcClassic.Constants.ModelConstants.Cpc664 => MachineCatalogConstants.CPC664Png,
        global::GWGUI.Emulation.Amstrad.Common.Machines.CpcClassic.Constants.ModelConstants.Cpc6128 => MachineCatalogConstants.CPC6128Png,
        global::GWGUI.Emulation.Amstrad.Common.Machines.CpcPlus.Constants.ModelConstants.Cpc464Plus => MachineCatalogConstants.CPC464PLUSPng,
        global::GWGUI.Emulation.Amstrad.Common.Machines.CpcPlus.Constants.ModelConstants.Cpc6128Plus => MachineCatalogConstants.CPC6128PLUSPng,
        global::GWGUI.Emulation.Amstrad.Common.Machines.Gx4000.Constants.ModelConstants.Gx4000 => MachineCatalogConstants.GX4000Png,
        _ => throw new ArgumentOutOfRangeException(nameof(machineId), machineId, null)
    };
}
