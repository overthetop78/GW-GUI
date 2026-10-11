using GWGUI.Emulation.Snk.Common.Interfaces;
using GWGUI.Emulation.Snk.Emulators.Common.Factories;

namespace GWGUI.Emulation.Snk.Emulators.Common.Dictionaries;

internal static class CoreCatalog
{
    private static readonly IReadOnlyList<MachineFactory> CatalogFactories = EmulatorCatalog.CreateAdapters().OfType<MachineFactory>().ToArray();
    private static readonly IReadOnlyDictionary<Emulator, Type> AdapterTypes = CatalogFactories.ToDictionary(adapter => adapter.Emulator, adapter => adapter.GetType());
    private static readonly IReadOnlyList<EmulatorCatalogEntry> Entries = CatalogFactories.Select(adapter => adapter.CatalogEntry) .OrderBy(entry => entry.Id, StringComparer.Ordinal).ToArray();

    private static readonly IReadOnlyDictionary<Emulator, EmulatorCatalogEntry> ByEmulator = Entries.ToDictionary(entry => entry.Emulator);
    private static readonly IReadOnlyDictionary<string, EmulatorCatalogEntry> ById = Entries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);

    public static IReadOnlyList<EmulatorCatalogEntry> All => Entries;

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => CatalogFactories.Select(adapter => CreateAdapter(adapter.Emulator)).ToArray();

    internal static IEmulatorAdapter CreateAdapter(Emulator emulator)
    {
        if (!AdapterTypes.TryGetValue(emulator, out var type)) throw new ArgumentOutOfRangeException(nameof(emulator));
        return (IEmulatorAdapter)Activator.CreateInstance(type, nonPublic: true)!;
    }

    public static EmulatorCatalogEntry Get(Emulator emulator) => ByEmulator.TryGetValue(emulator, out var entry) ? entry : throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);

    public static EmulatorCatalogEntry Get(MachineModel model) => Get(ConfigurationFunctions.DefaultEmulator(model));

    public static EmulatorCatalogEntry Get(string id) => ById.TryGetValue(id, out var entry) ? entry : throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static IReadOnlyList<EmulatorCatalogEntry> GetAll(MachineModel model) => Entries.Where(entry => entry.Models.Contains(model)).ToArray();

    public static EmulationEmulatorDefinition GetDefinition(EmulatorCatalogEntry entry) => new( entry.Id, entry.LibraryName, entry.DescriptionResourceKey ?? $"Emulation.Emulator.{entry.Id}.Description", entry.Models.Select(model => model.ToString()).ToHashSet(StringComparer.Ordinal));

    public static CoreInstallationPaths GetInstallationPaths(Emulator emulator, string installationRoot, string version) => EmulatorCatalogFunctions.GetInstallationPaths(Get(emulator), installationRoot, version);

    public static string GetActiveManifestPath(Emulator emulator, string installationRoot) => EmulatorCatalogFunctions.GetActiveManifestPath(Get(emulator), installationRoot);
}
