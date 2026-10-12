namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, IEmulatorInputMapper? mapper) => mapper?.MapInput(snapshot, input, model) ?? EmulationInputSnapshot.Empty;
}
