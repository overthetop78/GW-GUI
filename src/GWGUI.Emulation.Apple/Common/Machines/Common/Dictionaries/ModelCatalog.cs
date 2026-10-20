using GWGUI.Emulation;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = Array.AsReadOnly(
        HardwareModelCatalog.All.Select(definition => new EmulationMachineDefinition(definition.StableModelId, definition.DisplayNameResourceKey)).ToArray());

    public static MachineModel Parse(string id) => GWGUI.Emulation.Functions.EmulationMachineModelFunctions.Parse<MachineModel>(id);
}
