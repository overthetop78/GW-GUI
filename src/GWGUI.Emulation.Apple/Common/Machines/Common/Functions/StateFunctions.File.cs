namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class StateFileFunctions
{
    internal static void Write(string path, SavedStateHeader header, ReadOnlySpan<byte> state)
    {
        SavedStateFunctions.ValidatePayloadSize(state.Length);
        EmulationStateFileFunctions.WriteState(path, header, state);
    }

    internal static StateFile Read(string path)
    {
        var result = EmulationStateFileFunctions.ReadState<MachineModel, Emulator>(path,
            static () => SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateInvalid));
        SavedStateFunctions.ValidatePayloadSize(result.State.Length);
        return new StateFile(result.Header, result.State);
    }
}
