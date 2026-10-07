namespace GWGUI.Emulation.Nec.Common.Dictionaries;

public static class EmulatorCatalog
{
    internal static IReadOnlyList<EmulationEmulatorDefinition> All =>
        CreateAdapters().Select(adapter => adapter.Definition)
            .OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray();

    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() =>
    [
        new Emulators.BeetlePceFast.Factories.BeetlePceFastMachineFactory(),
        new Emulators.BeetleSgx.Factories.BeetleSgxMachineFactory(),
        new Emulators.BeetlePcfx.Factories.BeetlePcfxMachineFactory(),
        new Emulators.Geargrafx.Factories.GeargrafxMachineFactory(),
        new Emulators.BeetlePce.Factories.MachineFactory(),
        new Emulators.Quasi88.Factories.MachineFactory(),
        new Emulators.NekoProjectII.Factories.MachineFactory(),
        new Emulators.NP2Kai.Factories.MachineFactory(),
    ];

    public static string DefaultFor(string machineId) => machineId switch
    {
        Common.Machines.PcEngine.Constants.PcEngineMachineConstants.Id
            or Common.Machines.CoreGrafx.Constants.CoreGrafxMachineConstants.Id
            or Common.Machines.PcEngineDuo.Constants.PcEngineDuoMachineConstants.Id
            or Common.Machines.PcEngineLt.Constants.PcEngineLtMachineConstants.Id
            or Common.Machines.TurboExpress.Constants.TurboExpressMachineConstants.Id
            or Common.Machines.PcEngineCd.Constants.MachineConstants.Id
            or Common.Machines.PcEngineSuperCd.Constants.MachineConstants.Id
            or Common.Machines.PcEngineArcadeCard.Constants.MachineConstants.Id =>
            Emulators.BeetlePceFast.Constants.BeetlePceFastConstants.Id,
        Common.Machines.SuperGrafx.Constants.SuperGrafxMachineConstants.Id =>
            Emulators.BeetleSgx.Constants.BeetleSgxConstants.Id,
        Common.Machines.PcFx.Constants.PcFxMachineConstants.Id =>
            Emulators.BeetlePcfx.Constants.BeetlePcfxConstants.Id,
        Common.Machines.LaserActive.Constants.LaserActiveMachineConstants.Id =>
            Emulators.Geargrafx.Constants.GeargrafxConstants.Id,
        Common.Machines.Pc8001.Constants.MachineConstants.Id
            or Common.Machines.Pc8801.Constants.MachineConstants.Id
            or Common.Machines.Pc8801.Constants.MachineConstants.MkIIId
            or Common.Machines.Pc8801.Constants.MachineConstants.MkIISrId =>
            Emulators.Quasi88.Constants.CoreConstants.Id,
        Common.Machines.Pc9801.Constants.MachineConstants.Id
            or Common.Machines.Pc9821.Constants.MachineConstants.Id =>
            Emulators.NekoProjectII.Constants.CoreConstants.Id,
        _ => throw new ArgumentOutOfRangeException(nameof(machineId), machineId, null)
    };

    public static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) =>
        All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}

