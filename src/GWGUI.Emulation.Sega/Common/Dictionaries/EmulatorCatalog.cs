namespace GWGUI.Emulation.Sega.Common.Dictionaries;

public static class EmulatorCatalog
{
    internal static IReadOnlyList<EmulationEmulatorDefinition> All =>
        CreateAdapters().Select(adapter => adapter.Definition)
            .OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray();

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() =>
        typeof(EmulatorCatalog).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(IEmulatorAdapter).IsAssignableFrom(type)
                && type.Namespace?.Contains(".Emulators.", StringComparison.Ordinal) == true)
            .Select(type => (IEmulatorAdapter)Activator.CreateInstance(type, nonPublic: true)!)
            .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal)
            .ToArray();

    public static string DefaultFor(string machineId) => GetAll(machineId).FirstOrDefault()?.Id
        ?? throw new ArgumentOutOfRangeException(nameof(machineId), machineId, null);

    public static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) =>
        All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}

