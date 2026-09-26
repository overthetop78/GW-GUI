namespace GWGUI.Emulation.Atari.Emulators.Libretro.Dictionaries;

internal static class CoreCatalog
{
    private static readonly IReadOnlyList<EmulatorCatalogEntry> Entries =
        DiscoverFactories().Select(adapter => adapter.CatalogEntry)
            .OrderBy(entry => entry.Id, StringComparer.Ordinal).ToArray();

    private static readonly IReadOnlyDictionary<Emulator, EmulatorCatalogEntry> ByEmulator =
        Entries.ToDictionary(entry => entry.Emulator);
    private static readonly IReadOnlyDictionary<string, EmulatorCatalogEntry> ById =
        Entries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);
    private static readonly IReadOnlyDictionary<MachineModel, Emulator> ByModel =
        EmulatorCatalogFunctions.CreateModelAssociations(Entries);

    public static IReadOnlyList<EmulatorCatalogEntry> All => Entries;

    private static IReadOnlyList<MachineFactory> DiscoverFactories() =>
        typeof(CoreCatalog).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(MachineFactory).IsAssignableFrom(type)
                && type.Namespace?.Contains(".Emulators.", StringComparison.Ordinal) == true)
            .Select(type => (MachineFactory)Activator.CreateInstance(type, nonPublic: true)!)
            .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal)
            .ToArray();

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => DiscoverFactories();

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
