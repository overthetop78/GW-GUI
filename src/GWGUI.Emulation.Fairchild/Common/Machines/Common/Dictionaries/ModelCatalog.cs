namespace GWGUI.Emulation.Fairchild.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = Array.AsReadOnly(Enum.GetValues<MachineModel>().Select(model => new EmulationMachineDefinition(model.ToString(), $"Emulation.Fairchild.Model.{model}")).ToArray());

    public static MachineModel Parse(string id) => Enum.TryParse<MachineModel>(id, out var model) && Enum.IsDefined(model) ? model : throw new ArgumentOutOfRangeException(nameof(id), id, null);
}
