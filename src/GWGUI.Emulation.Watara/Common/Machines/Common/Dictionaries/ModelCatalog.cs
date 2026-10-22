using CommonCoreHostFunctions = GWGUI.Emulation.Watara.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Watara.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = Array.AsReadOnly(Enum.GetValues<MachineModel>().Select(model => new EmulationMachineDefinition(model.ToString(), $"Emulation.Watara.Model.{model}")).ToArray());
    public static MachineModel Parse(string id) => CommonCoreHostFunctions.Adapters.Parse<MachineModel>(id);
}
