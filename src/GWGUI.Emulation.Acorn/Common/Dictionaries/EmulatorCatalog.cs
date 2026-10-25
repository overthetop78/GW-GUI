using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Acorn.Common.Dictionaries;

internal static class EmulatorCatalog
{
    internal static IReadOnlyList<EmulationEmulatorDefinition> All => CreateAdapters().Select(adapter => adapter.Definition).OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray();

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() =>
        CommonCoreHostFunctions.Adapters.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly)
            .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal)
            .ToArray();

    internal static IEmulatorAdapter CreateAdapter(Emulator emulator) => CreateAdapters().SingleOrDefault(adapter => string.Equals(adapter.EmulatorKey, emulator.ToString(), StringComparison.Ordinal)) ?? throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);

    internal static EmulationEmulatorDefinition Get(Emulator emulator) => CreateAdapter(emulator).Definition;

    internal static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) => All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}
