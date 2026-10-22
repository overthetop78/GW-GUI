using CommonCoreHostFunctions = GWGUI.Emulation.Sega.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Sega.Common.Dictionaries;

public static class EmulatorCatalog
{
    internal static IReadOnlyList<EmulationEmulatorDefinition> All => CreateAdapters().Select(adapter => adapter.Definition).ToArray();

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => CommonCoreHostFunctions.Adapters.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly);

    public static string DefaultFor(string machineId) => CreateAdapters()
        .FirstOrDefault(adapter => adapter is IEmulatorMachineDefaults defaults && defaults.IsDefaultFor(machineId))?.EmulatorId
        ?? throw new ArgumentOutOfRangeException(nameof(machineId), machineId, null);

    public static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) => All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}
