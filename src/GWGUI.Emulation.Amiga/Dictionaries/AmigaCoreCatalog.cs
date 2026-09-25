namespace GWGUI.Emulation.Amiga.Dictionaries;

internal static class AmigaCoreCatalog
{
    private static readonly EmulationEmulatorDefinition Puae = new(
        AmigaEmulationModuleConstants.Puae,
        "PUAE",
        "Emulation.Emulator.puae.Description",
        AmigaMachineCatalog.All.Select(machine => machine.Id).ToHashSet(StringComparer.Ordinal));

    internal static IReadOnlyList<EmulationEmulatorDefinition> All => [Puae];

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => [new PuaeMachineFactory()];

    internal static EmulationEmulatorDefinition Get(string emulatorId) =>
        string.Equals(emulatorId, Puae.Id, StringComparison.Ordinal)
            ? Puae
            : throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);

    internal static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) =>
        Puae.MachineIds.Contains(machineId) ? [Puae] : [];
}
