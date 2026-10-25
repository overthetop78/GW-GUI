using CommonCoreHostFunctions = GWGUI.Emulation.Coleco.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Coleco.Emulators.Common.Contracts;
using GWGUI.Emulation.Coleco.Emulators.Common.Exceptions;
using GWGUI.Emulation.Coleco.Common.Functions;
using GWGUI.Emulation.Coleco.Common.Interfaces;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Dictionaries;

internal static class CoreCatalog
{
    private static readonly IReadOnlyList<EmulatorCatalogEntry> Entries = DiscoverFactories().Select(adapter => adapter.CatalogEntry).OrderBy(entry => entry.Id, StringComparer.Ordinal).ToArray();

    private static readonly IReadOnlyDictionary<Emulator, EmulatorCatalogEntry> ByEmulator = Entries.ToDictionary(entry => entry.Emulator);
    private static readonly IReadOnlyDictionary<string, EmulatorCatalogEntry> ById = Entries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);
    private static readonly IReadOnlyDictionary<MachineModel, Emulator> ByModel = EmulatorCatalogFunctions.CreateModelAssociations(Entries.OrderBy(entry => entry.Emulator).ToArray());

    public static IReadOnlyList<EmulatorCatalogEntry> All => Entries;

    private static IReadOnlyList<MachineFactory> DiscoverFactories() =>
        CommonCoreHostFunctions.Adapters.Create<MachineFactory, IEmulatorAdapter>(typeof(CoreCatalog).Assembly)
            .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal)
            .ToArray();

    public static EmulatorCatalogEntry Get(Emulator emulator) => ByEmulator.TryGetValue(emulator, out var entry) ? entry : throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);

    public static EmulatorCatalogEntry Get(MachineModel model) => ByModel.TryGetValue(model, out var emulator) ? Get(emulator) : throw new ArgumentOutOfRangeException(nameof(model), model, ErrorMessages.MissingModel);

    public static EmulatorCatalogEntry Get(string id) => ById.TryGetValue(id, out var entry) ? entry : throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static IReadOnlyList<EmulatorCatalogEntry> GetAll(MachineModel model) => Entries.Where(entry => entry.Models.Contains(model)).ToArray();

    public static EmulationEmulatorDefinition GetDefinition(EmulatorCatalogEntry entry) => CommonCoreHostFunctions.Adapters.ToDefinition(entry);

    public static CoreInstallationPaths GetInstallationPaths(Emulator emulator, string installationRoot, string version) => EmulatorCatalogFunctions.GetInstallationPaths(Get(emulator), installationRoot, version);

    public static string GetActiveManifestPath(Emulator emulator, string installationRoot) => EmulatorCatalogFunctions.GetActiveManifestPath(Get(emulator), installationRoot);
}
