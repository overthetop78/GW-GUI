namespace GWGUI.Emulation.Philips.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, IEmulatorInputMapper? mapper) => mapper is not null
        ? mapper.MapInput(snapshot, input, model)
        : snapshot with
        {
            Controllers = [],
            Pointer = snapshot.Pointer with { Left = false, Right = false, Middle = false, ExtendedButton1 = false, ExtendedButton2 = false }
        };
}
