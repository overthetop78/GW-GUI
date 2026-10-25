using CommonCoreHostFunctions = GWGUI.Emulation.Sharp.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = Array.AsReadOnly(Enum.GetValues<MachineModel>().Select(model => new EmulationMachineDefinition(model.ToString(), $"Emulation.Sharp.Model.{model}")).ToArray());

    public static MachineModel Parse(string id) => CommonCoreHostFunctions.Adapters.Parse<MachineModel>(id);
}
