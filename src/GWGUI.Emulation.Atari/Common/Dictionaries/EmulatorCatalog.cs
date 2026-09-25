namespace GWGUI.Emulation.Atari.Common.Dictionaries;

public static class EmulatorCatalog
{
    private static readonly IReadOnlyList<EmulatorCatalogEntry> Entries =
    [
        EmulatorCatalogFunctions.Create(Emulator.Hatari, EmulatorCatalogConstants.HatariId,
            CoreIdentityConstants.Hatari, EmulatorCatalogConstants.HatariDllName,
            EmulatorCatalogConstants.HatariSource, EmulatorCatalogConstants.HatariRevision,
            MachineModel.St, MachineModel.Stf, MachineModel.Stfm, MachineModel.MegaSt,
            MachineModel.Ste, MachineModel.MegaSte, MachineModel.Tt, MachineModel.Falcon),
        EmulatorCatalogFunctions.Create(Emulator.Atari800, EmulatorCatalogConstants.Atari800Id,
            CoreIdentityConstants.Atari800, EmulatorCatalogConstants.Atari800DllName,
            EmulatorCatalogConstants.Atari800Source, EmulatorCatalogConstants.Atari800Revision,
            MachineModel.Atari400, MachineModel.Atari800, MachineModel.Atari800Xl,
            MachineModel.Atari130Xe, MachineModel.Xegs, MachineModel.XlXe,
            MachineModel.Atari5200),
        EmulatorCatalogFunctions.Create(Emulator.Stella, EmulatorCatalogConstants.StellaId,
            CoreIdentityConstants.Stella, EmulatorCatalogConstants.StellaDllName,
            EmulatorCatalogConstants.StellaSource, EmulatorCatalogConstants.StellaRevision,
            MachineModel.Atari2600),
        EmulatorCatalogFunctions.Create(Emulator.ProSystem, EmulatorCatalogConstants.ProSystemId,
            CoreIdentityConstants.ProSystem, EmulatorCatalogConstants.ProSystemDllName,
            EmulatorCatalogConstants.ProSystemSource, EmulatorCatalogConstants.ProSystemRevision,
            MachineModel.Atari7800),
        EmulatorCatalogFunctions.Create(Emulator.BeetleLynx, EmulatorCatalogConstants.BeetleLynxId,
            CoreIdentityConstants.BeetleLynx, EmulatorCatalogConstants.BeetleLynxDllName,
            EmulatorCatalogConstants.BeetleLynxSource, EmulatorCatalogConstants.BeetleLynxRevision,
            MachineModel.Lynx),
        EmulatorCatalogFunctions.Create(Emulator.VirtualJaguar, EmulatorCatalogConstants.VirtualJaguarId,
            CoreIdentityConstants.VirtualJaguar, EmulatorCatalogConstants.VirtualJaguarDllName,
            EmulatorCatalogConstants.VirtualJaguarSource, EmulatorCatalogConstants.VirtualJaguarRevision,
            MachineModel.Jaguar, MachineModel.JaguarCd)
    ];

    private static readonly IReadOnlyDictionary<Emulator, EmulatorCatalogEntry> ByEmulator =
        Entries.ToDictionary(entry => entry.Emulator);
    private static readonly IReadOnlyDictionary<string, EmulatorCatalogEntry> ById =
        Entries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);
    private static readonly IReadOnlyDictionary<MachineModel, Emulator> ByModel =
        EmulatorCatalogFunctions.CreateModelAssociations(Entries);

    public static IReadOnlyList<EmulatorCatalogEntry> All => Entries;

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() =>
        typeof(EmulatorCatalog).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(IEmulatorAdapter).IsAssignableFrom(type)
                && type.Namespace?.Contains(".Emulators.", StringComparison.Ordinal) == true)
            .Select(type => (IEmulatorAdapter)Activator.CreateInstance(type, nonPublic: true)!)
            .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal)
            .ToArray();

    internal static IEmulatorAdapter CreateAdapter(Emulator emulator)
    {
        var id = Get(emulator).Id;
        return CreateAdapters().Single(adapter => string.Equals(adapter.EmulatorId, id, StringComparison.Ordinal));
    }

    public static EmulatorCatalogEntry Get(Emulator emulator) => ByEmulator.TryGetValue(emulator, out var entry)
        ? entry
        : throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);

    public static EmulatorCatalogEntry Get(MachineModel model) => ByModel.TryGetValue(model, out var emulator)
        ? Get(emulator)
        : throw new ArgumentOutOfRangeException(nameof(model), model, EmulatorCatalogErrors.MissingModel);

    public static EmulatorCatalogEntry Get(string id) => ById.TryGetValue(id, out var entry)
        ? entry
        : throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static IReadOnlyList<EmulatorCatalogEntry> GetAll(MachineModel model) =>
        Entries.Where(entry => entry.Models.Contains(model)).ToArray();

    public static EmulationEmulatorDefinition GetDefinition(EmulatorCatalogEntry entry) => new(
        entry.Id, entry.LibraryName, $"Emulation.Emulator.{entry.Id}.Description",
        entry.Models.Select(model => model.ToString()).ToHashSet(StringComparer.Ordinal));

    public static CoreInstallationPaths GetInstallationPaths(Emulator emulator,
        string installationRoot, string version) =>
        EmulatorCatalogFunctions.GetInstallationPaths(Get(emulator), installationRoot, version);

    public static string GetActiveManifestPath(Emulator emulator, string installationRoot) =>
        EmulatorCatalogFunctions.GetActiveManifestPath(Get(emulator), installationRoot);
}
