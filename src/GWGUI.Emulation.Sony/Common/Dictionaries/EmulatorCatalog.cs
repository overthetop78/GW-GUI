using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Sony.Common.Dictionaries;

public static class EmulatorCatalog
{
    internal static IReadOnlyList<EmulationEmulatorDefinition> All => CreateAdapters().Select(adapter => adapter.Definition).OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray();

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => CommonCoreHostFunctions.Adapters.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly)
        .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).ToArray();


    public static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) => All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}

