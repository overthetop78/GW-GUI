using GWGUI.Emulation;
using GWGUI.Emulation.Nec.Common.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineLt.Constants;
using GWGUI.Emulation.Nec.Common.Machines.LaserActive.Constants;

namespace GWGUI.Emulation.Nec.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = ModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id,
            MachineConfigurationConstants.ResourcePrefix + model.Id,
            model.Id is PcEngineLtMachineConstants.Id or LaserActiveMachineConstants.Id
                ? EmulationModuleConstants.BrandAssetResourceName
                : $"{EmulationModuleConstants.AssetResourcePrefix}.Machines.{model.Id}.png"))
        .ToArray();

}
