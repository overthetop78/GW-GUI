using CommonCoreHostFunctions = GWGUI.Emulation.Commodore.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Commodore.Common.Dictionaries;

internal static class EmulatorCatalog
{
    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() =>
        CommonCoreHostFunctions.Adapters.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly).OrderBy(adapter => Enum.Parse<Emulator>(adapter.EmulatorKey)).ToArray();

    internal static IReadOnlyList<EmulationEmulatorDefinition> All => CreateAdapters().Select(adapter => adapter.Definition).ToArray();

    internal static IEmulatorAdapter CreateAdapter(Emulator emulator) => CreateAdapters().SingleOrDefault(adapter => string.Equals(adapter.EmulatorKey, emulator.ToString(), StringComparison.Ordinal)) ?? throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);

    internal static EmulationEmulatorDefinition Get(Emulator emulator) => CreateAdapter(emulator).Definition;

    internal static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) => All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();

    internal static Emulator DefaultFor(string machineId)
    {
        _ = ModelCatalog.Get(machineId);
        var adapter = CreateAdapters().FirstOrDefault(item => item.Definition.MachineIds.Contains(machineId)) ?? throw new ArgumentOutOfRangeException(nameof(machineId), machineId, null);
        return Enum.Parse<Emulator>(adapter.EmulatorKey);
    }
}
