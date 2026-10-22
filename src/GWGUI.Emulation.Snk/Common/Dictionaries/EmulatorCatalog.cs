using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Snk.Common.Dictionaries;
internal static class EmulatorCatalog
{
    private static readonly IReadOnlyList<IEmulatorAdapter> DiscoveredAdapters = DiscoverAdapters();
    private static readonly IReadOnlyDictionary<Emulator, Type> AdapterTypes = DiscoveredAdapters.ToDictionary(adapter => Enum.Parse<Emulator>(adapter.EmulatorKey), adapter => adapter.GetType());
    private static readonly IReadOnlyList<EmulatorDefinition> Entries = DiscoveredAdapters.Select(adapter => new EmulatorDefinition(Enum.Parse<Emulator>(adapter.EmulatorKey),
        adapter.Definition.Id,
        adapter.Definition.DisplayName,
        adapter.Definition.DescriptionResourceKey,
        adapter.Definition.MachineIds.Select(Enum.Parse<MachineModel>).ToHashSet())).OrderBy(entry => entry.Id,
        StringComparer.Ordinal).ToArray();
    private static readonly IReadOnlyDictionary<Emulator, EmulatorDefinition> ByEmulator = Entries.ToDictionary(entry => entry.Emulator);
    private static readonly IReadOnlyDictionary<string, EmulatorDefinition> ById = Entries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);
    internal static IReadOnlyList<EmulatorDefinition> All => Entries;

    private static IReadOnlyList<IEmulatorAdapter> DiscoverAdapters() => CommonCoreHostFunctions.Adapters.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly).OrderBy(adapter => adapter.EmulatorId,
        StringComparer.Ordinal).ToArray();
    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => Entries.Select(entry => CreateAdapter(entry.Emulator)).ToArray();
    internal static IEmulatorAdapter CreateAdapter(Emulator emulator) => AdapterTypes.TryGetValue(emulator, out var type) ? (IEmulatorAdapter)Activator.CreateInstance(type, nonPublic: true)! : throw new ArgumentOutOfRangeException(nameof(emulator));
    internal static EmulatorDefinition Get(Emulator emulator) => ByEmulator.TryGetValue(emulator, out var entry) ? entry : throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);
}