using PUAEAdapter = GWGUI.Emulation.Commodore.Emulators.PUAE.Factories.PuaeMachineFactory;
using PUAE2021Adapter = GWGUI.Emulation.Commodore.Emulators.PUAE2021.Factories.PuaeMachineFactory;
using AmiberryAdapter = GWGUI.Emulation.Commodore.Emulators.Amiberry.Factories.AmiberryMachineFactory;
using FrodoAdapter = GWGUI.Emulation.Commodore.Emulators.Frodo.Factories.MachineFactory;
using ViceX64Adapter = GWGUI.Emulation.Commodore.Emulators.ViceX64.Factories.MachineFactory;
using ViceX64ScAdapter = GWGUI.Emulation.Commodore.Emulators.ViceX64Sc.Factories.MachineFactory;
using ViceX64DtvAdapter = GWGUI.Emulation.Commodore.Emulators.ViceX64Dtv.Factories.MachineFactory;
using ViceXScpu64Adapter = GWGUI.Emulation.Commodore.Emulators.ViceXScpu64.Factories.MachineFactory;
using ViceX128Adapter = GWGUI.Emulation.Commodore.Emulators.ViceX128.Factories.MachineFactory;
using ViceXCbm5x0Adapter = GWGUI.Emulation.Commodore.Emulators.ViceXCbm5x0.Factories.MachineFactory;
using ViceXCbm2Adapter = GWGUI.Emulation.Commodore.Emulators.ViceXCbm2.Factories.MachineFactory;
using ViceXPetAdapter = GWGUI.Emulation.Commodore.Emulators.ViceXPet.Factories.MachineFactory;
using ViceXPlus4Adapter = GWGUI.Emulation.Commodore.Emulators.ViceXPlus4.Factories.MachineFactory;
using ViceXVicAdapter = GWGUI.Emulation.Commodore.Emulators.ViceXVic.Factories.MachineFactory;

namespace GWGUI.Emulation.Commodore.Common.Dictionaries;

internal static class EmulatorCatalog
{
    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() =>
    [
        new PUAEAdapter(),
        new PUAE2021Adapter(),
        new AmiberryAdapter(),
        new FrodoAdapter(),
        new ViceX64Adapter(),
        new ViceX64ScAdapter(),
        new ViceX64DtvAdapter(),
        new ViceXScpu64Adapter(),
        new ViceX128Adapter(),
        new ViceXCbm5x0Adapter(),
        new ViceXCbm2Adapter(),
        new ViceXPetAdapter(),
        new ViceXPlus4Adapter(),
        new ViceXVicAdapter()
    ];

    internal static IReadOnlyList<EmulationEmulatorDefinition> All =>
        CreateAdapters().Select(adapter => adapter.Definition).ToArray();

    internal static IEmulatorAdapter CreateAdapter(Emulator emulator) =>
        CreateAdapters().SingleOrDefault(adapter => string.Equals(
            adapter.EmulatorKey, emulator.ToString(), StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);

    internal static EmulationEmulatorDefinition Get(Emulator emulator) =>
        CreateAdapter(emulator).Definition;

    internal static EmulationEmulatorDefinition Get(string emulatorId) =>
        CreateAdapters().SingleOrDefault(adapter => string.Equals(
            adapter.EmulatorId, emulatorId, StringComparison.Ordinal))?.Definition
        ?? throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);

    internal static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) =>
        All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();

    internal static Emulator DefaultFor(string machineId)
    {
        _ = ModelCatalog.Get(machineId);
        var adapter = CreateAdapters().FirstOrDefault(item => item.Definition.MachineIds.Contains(machineId))
            ?? throw new ArgumentOutOfRangeException(nameof(machineId), machineId, null);
        return Enum.Parse<Emulator>(adapter.EmulatorKey);
    }
}
