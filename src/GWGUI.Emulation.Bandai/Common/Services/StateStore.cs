namespace GWGUI.Emulation.Bandai.Common.Services;

internal static class StateStore
{
    internal static void Write(string path, SavedStateHeader header, ReadOnlySpan<byte> state)
    {
        SavedStateFunctions.ValidatePayloadSize(state.Length);
        EmulationSdk.StateFiles.WriteState(path, header, state);
    }

    internal static StateFile Read(string path)
    {
        var result = EmulationSdk.StateFiles.ReadState<MachineModel, Emulator>(path,
            static () => SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateInvalid));
        SavedStateFunctions.ValidatePayloadSize(result.State.Length);
        return new StateFile(result.Header, result.State);
    }
}
