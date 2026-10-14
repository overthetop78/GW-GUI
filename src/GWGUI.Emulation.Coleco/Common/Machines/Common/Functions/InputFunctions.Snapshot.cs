namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core, IEmulatorInputMapper? mapper = null)
    {
        mapper ??= new Engine().Adapter(core) as IEmulatorInputMapper;
        return mapper?.MapInput(snapshot, input, model) ?? EmulationInputSnapshot.Empty;
    }
}
