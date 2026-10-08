namespace GWGUI.Emulation.Sony.Common.Dictionaries;

public static class EmulatorCatalog
{
    internal static IReadOnlyList<EmulationEmulatorDefinition> All =>
        CreateAdapters().Select(adapter => adapter.Definition)
            .OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray();

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() =>
    [
        new Emulators.Pcsx2.Factories.Pcsx2MachineFactory(),
        new Emulators.Ppsspp.Factories.PpssppMachineFactory(),
        new Emulators.SwanStation.Factories.SwanStationMachineFactory()
    ];

    public static string DefaultFor(string machineId) => GetAll(machineId).FirstOrDefault()?.Id
        ?? throw new NotSupportedException($"No Sony emulator adapter is installed for '{machineId}'.");

    public static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) =>
        All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}

