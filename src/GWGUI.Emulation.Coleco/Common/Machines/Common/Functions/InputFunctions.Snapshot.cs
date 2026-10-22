namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, MachineConfiguration configuration, IEmulatorInputMapper? mapper = null)
    {
        mapper ??= new Engine().Adapter(configuration.Core) as IEmulatorInputMapper;
        return mapper?.MapInput(snapshot, configuration) ?? EmulationInputSnapshot.Empty;
    }
}
