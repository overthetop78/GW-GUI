using CommonCoreHostFunctions = GWGUI.Emulation.Sinclair.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = Array.AsReadOnly(Enum.GetValues<MachineModel>().Select(model => new EmulationMachineDefinition(model.ToString(), $"Emulation.Sinclair.Model.{model}")).ToArray());
    public static MachineModel Parse(string id) => CommonCoreHostFunctions.Adapters.Parse<MachineModel>(id);
}
