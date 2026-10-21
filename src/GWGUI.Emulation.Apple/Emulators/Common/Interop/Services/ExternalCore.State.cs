namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    public byte[] SaveState() => ExternalCoreLifecycleFunctions.SerializeState(RequireExports, SupportsSaveStates,
        SavedStateConstants.MaximumStateSize,
        static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateUnavailable),
        static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateSaveFailed));

    public void LoadState(ReadOnlySpan<byte> state) => ExternalCoreLifecycleFunctions.UnserializeState(RequireExports,
        SupportsSaveStates, SavedStateConstants.MaximumStateSize, state,
        static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateSizeInvalid),
        static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateLoadFailed));
}