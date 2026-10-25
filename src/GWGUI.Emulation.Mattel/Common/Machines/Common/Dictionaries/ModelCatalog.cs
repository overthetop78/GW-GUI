using CommonCoreHostFunctions = GWGUI.Emulation.Mattel.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = Array.AsReadOnly(Enum.GetValues<MachineModel>().Select(model => new EmulationMachineDefinition(model.ToString(), $"Emulation.Mattel.Model.{model}")).ToArray());

    public static MachineModel Parse(string id) => CommonCoreHostFunctions.Adapters.Parse<MachineModel>(id);
}
