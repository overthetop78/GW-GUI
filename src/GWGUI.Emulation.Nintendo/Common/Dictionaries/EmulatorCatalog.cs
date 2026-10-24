using CommonCoreHostFunctions = GWGUI.Emulation.Nintendo.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Nintendo.Common.Dictionaries;

public static class EmulatorCatalog
{
    internal static IReadOnlyList<EmulationEmulatorDefinition> All => CreateAdapters().Select(adapter => adapter.Definition).ToArray();

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => CommonCoreHostFunctions.Adapters.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly);

    public static string DefaultEmulator(string machineId) => CreateAdapters()
        .FirstOrDefault(adapter => adapter.Definition.MachineIds.Contains(machineId) && adapter.IsDefaultFor(machineId))?.EmulatorId
        ?? throw new NotSupportedException(machineId);

    public static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) => All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}
