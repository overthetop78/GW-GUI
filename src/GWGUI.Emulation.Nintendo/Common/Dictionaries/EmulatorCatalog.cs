namespace GWGUI.Emulation.Nintendo.Common.Dictionaries;

public static class EmulatorCatalog
{
    internal static IReadOnlyList<EmulationEmulatorDefinition> All => CreateAdapters().Select(adapter => adapter.Definition).ToArray();

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => EmulationAdapterFunctions.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly);

    public static string DefaultFor(string machineId) => CreateAdapters()
        .FirstOrDefault(adapter => adapter is IEmulatorMachineDefaults defaults && defaults.IsDefaultFor(machineId))?.EmulatorId
        ?? throw new NotSupportedException(machineId);

    public static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) => All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}
