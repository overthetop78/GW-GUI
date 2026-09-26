using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } =
    [
        ..StModelCatalog.All.Select(definition =>
            Model(definition.Model, definition.DisplayNameResourceKey)),
        ..HardwareModelCatalog.All.Select(definition =>
            Model(definition.Model, definition.DisplayNameResourceKey))
    ];

    public static MachineModel Parse(string id) =>
        Enum.TryParse<MachineModel>(id, out var model) && Enum.IsDefined(model)
            ? model
            : throw new ArgumentOutOfRangeException(nameof(id), id, null);

    private static EmulationMachineDefinition Model(MachineModel model, string resourceKey) =>
        new(model.ToString(), resourceKey);
}
