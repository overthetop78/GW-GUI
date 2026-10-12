namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core) => snapshot with { Keys = KeyboardDictionary.Apply(snapshot.Keys, input, model), Pointer = EmulationInputSnapshot.Empty.Pointer, Controllers = [] };
}