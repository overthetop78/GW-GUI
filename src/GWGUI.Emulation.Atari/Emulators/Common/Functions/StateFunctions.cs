namespace GWGUI.Emulation.Atari.Emulators.Common.Functions;

internal static class StateFunctions
{
    internal static bool IsAvailable(ExternalCoreExports exports)
    {
        var size = exports.GetSerializedSize();
        return size > nuint.Zero && size <= SavedStateConstants.MaximumStateSize;
    }
}

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
