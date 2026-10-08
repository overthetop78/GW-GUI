namespace GWGUI.Emulation.Sega.Common.Dictionaries;

public static class EmulatorCatalog
{
    internal static IReadOnlyList<EmulationEmulatorDefinition> All => CreateAdapters().Select(adapter => adapter.Definition).ToArray();
    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() =>
    [
        new Emulators.BlastEm.Factories.MachineFactory(),
        new Emulators.ClownMDEmu.Factories.MachineFactory(),
        new Emulators.Flycast.Factories.MachineFactory(),
        new Emulators.Gearsystem.Factories.MachineFactory(),
        new Emulators.GenesisPlusGX.Factories.MachineFactory(),
        new Emulators.GenesisPlusGXWide.Factories.MachineFactory(),
        new Emulators.Kronos.Factories.MachineFactory(),
        new Emulators.BeetleSaturn.Factories.MachineFactory(),
        new Emulators.PicoDrive.Factories.MachineFactory(),
        new Emulators.SmsPlusGX.Factories.MachineFactory(),
        new Emulators.Supermodel.Factories.MachineFactory(),
        new Emulators.VeMUlator.Factories.MachineFactory(),
        new Emulators.YabaSanshiro.Factories.MachineFactory(),
        new Emulators.Yabause.Factories.MachineFactory(),
        new Emulators.Ymir.Factories.MachineFactory(),
        new Emulators.BlueMSX.Factories.MachineFactory(),
    ];
    public static string DefaultFor(string machineId) => machineId switch
    {
        ModelConstants.Sg1000 => Emulators.GenesisPlusGX.Constants.CoreConstants.Id,
        ModelConstants.Sc3000 => Emulators.GenesisPlusGX.Constants.CoreConstants.Id,
        ModelConstants.Sf7000 => Emulators.BlueMSX.Constants.CoreConstants.Id,
        ModelConstants.MarkIII => Emulators.GenesisPlusGX.Constants.CoreConstants.Id,
        ModelConstants.MasterSystem => Emulators.GenesisPlusGX.Constants.CoreConstants.Id,
        ModelConstants.MegaDrive => Emulators.GenesisPlusGX.Constants.CoreConstants.Id,
        ModelConstants.Pico => Emulators.GenesisPlusGX.Constants.CoreConstants.Id,
        ModelConstants.GameGear => Emulators.GenesisPlusGX.Constants.CoreConstants.Id,
        ModelConstants.MegaCd => Emulators.GenesisPlusGX.Constants.CoreConstants.Id,
        ModelConstants.ThirtyTwoX => Emulators.PicoDrive.Constants.CoreConstants.Id,
        ModelConstants.Saturn => Emulators.Yabause.Constants.CoreConstants.Id,
        ModelConstants.Dreamcast => Emulators.Flycast.Constants.CoreConstants.Id,
        ModelConstants.Naomi => Emulators.Flycast.Constants.CoreConstants.Id,
        ModelConstants.Naomi2 => Emulators.Flycast.Constants.CoreConstants.Id,
        ModelConstants.Atomiswave => Emulators.Flycast.Constants.CoreConstants.Id,
        ModelConstants.SystemSp => Emulators.Flycast.Constants.CoreConstants.Id,
        ModelConstants.StV => Emulators.Kronos.Constants.CoreConstants.Id,
        ModelConstants.Model3 => Emulators.Supermodel.Constants.CoreConstants.Id,
        ModelConstants.DreamcastVmu => Emulators.VeMUlator.Constants.CoreConstants.Id,
        _ => throw new ArgumentOutOfRangeException(nameof(machineId), machineId, null)
    };
    public static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) =>
        All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}
