namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, MachineConfiguration configuration, IEmulatorInputMapper? mapper) => mapper?.MapInput(snapshot, configuration) ?? EmulationInputSnapshot.Empty;
}
