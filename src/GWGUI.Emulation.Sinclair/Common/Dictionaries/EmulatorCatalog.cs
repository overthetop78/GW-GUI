using CommonCoreHostFunctions = GWGUI.Emulation.Sinclair.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Sinclair.Common.Dictionaries;

internal static class EmulatorCatalog
{
    private sealed record Registration(Type AdapterType, Emulator Emulator, EmulationEmulatorDefinition Definition);

    private static readonly IReadOnlyList<Registration> Registrations = Discover();
    private static readonly IReadOnlyDictionary<Emulator, EmulationEmulatorDefinition> Definitions = Registrations.ToDictionary(entry => entry.Emulator, entry => entry.Definition);

    private static IReadOnlyList<Registration> Discover() => CommonCoreHostFunctions.Adapters.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly)
        .Select(adapter => new Registration(adapter.GetType(), Enum.Parse<Emulator>(adapter.EmulatorKey), adapter.Definition))
        .OrderBy(entry => entry.Definition.Id, StringComparer.Ordinal)
        .ToArray();

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => Registrations.Select(entry => (IEmulatorAdapter)Activator.CreateInstance(entry.AdapterType, nonPublic: true)!).ToArray();

    internal static EmulationEmulatorDefinition GetDefinition(Emulator emulator) => Definitions.TryGetValue(emulator, out var definition) ? definition : throw new ArgumentOutOfRangeException(nameof(emulator));

    internal static Emulator DefaultEmulator(MachineModel model) => Registrations.Where(entry => entry.Definition.MachineIds.Contains(model.ToString())).OrderBy(entry => entry.Emulator).Select(entry => (Emulator?)entry.Emulator) .FirstOrDefault() ?? Emulator.None;
}
