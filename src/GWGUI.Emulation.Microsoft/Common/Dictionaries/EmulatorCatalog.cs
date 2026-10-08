namespace GWGUI.Emulation.Microsoft.Common.Dictionaries;

public static class EmulatorCatalog
{
    internal static IReadOnlyList<EmulationEmulatorDefinition> All =>
        CreateAdapters().Select(adapter => adapter.Definition)
            .OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray();

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() =>
        [];

    public static string DefaultFor(string machineId) => GetAll(machineId).FirstOrDefault()?.Id
        ?? throw new NotSupportedException($"No Microsoft emulator adapter is installed for '{machineId}'.");

    public static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) =>
        All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}

