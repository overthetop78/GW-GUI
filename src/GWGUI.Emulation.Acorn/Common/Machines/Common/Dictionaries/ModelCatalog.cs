using GWGUI.Emulation;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } =
        HardwareModelCatalog.All.Select(definition => new EmulationMachineDefinition(definition.Model.ToString(), definition.DisplayNameResourceKey)).ToArray();

    public static MachineModel Parse(string id) => Enum.TryParse<MachineModel>(id, out var model) && Enum.IsDefined(model) ? model : throw new ArgumentOutOfRangeException(nameof(id), id, null);
}
