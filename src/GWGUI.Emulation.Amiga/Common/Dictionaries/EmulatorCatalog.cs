namespace GWGUI.Emulation.Amiga.Common.Dictionaries;

internal static class EmulatorCatalog
{
    private static readonly EmulationEmulatorDefinition Puae = new(
        EmulationModuleConstants.Puae,
        "PUAE",
        "Emulation.Emulator.puae.Description",
        MachineCatalog.All.Select(machine => machine.Id).ToHashSet(StringComparer.Ordinal));

    internal static IReadOnlyList<EmulationEmulatorDefinition> All => [Puae];

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() =>
        typeof(EmulatorCatalog).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(IEmulatorAdapter).IsAssignableFrom(type)
                && type.Namespace?.Contains(".Emulators.", StringComparison.Ordinal) == true)
            .Select(type => (IEmulatorAdapter)Activator.CreateInstance(type, nonPublic: true)!)
            .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal)
            .ToArray();

    internal static EmulationEmulatorDefinition Get(Emulator emulator) => emulator switch
    {
        Emulator.External => Puae,
        _ => throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null)
    };

    internal static EmulationEmulatorDefinition Get(string emulatorId) =>
        string.Equals(emulatorId, Puae.Id, StringComparison.Ordinal)
            ? Puae
            : throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);

    internal static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) =>
        Puae.MachineIds.Contains(machineId) ? [Puae] : [];
}
