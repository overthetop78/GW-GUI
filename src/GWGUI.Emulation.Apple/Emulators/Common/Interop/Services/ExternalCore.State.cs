using GWGUI.Emulation.Interop;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    public byte[] SaveState()
    {
        var exports = RequireExports();
        var size = exports.GetSerializedSize();
        if (!SupportsSaveStates || size == nuint.Zero || size > SavedStateConstants.MaximumStateSize)
            throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateUnavailable);
        var length = checked((int)size);
        var buffer = Marshal.AllocHGlobal(length);
        try
        {
            if (!exports.Serialize(buffer, size))
                throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateSaveFailed);
            var state = new byte[length];
            Marshal.Copy(buffer, state, BufferConstants.FirstBufferIndex, length);
            return state;
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        if (!SupportsSaveStates || state.IsEmpty || state.Length > SavedStateConstants.MaximumStateSize)
            throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateSizeInvalid);
        var exports = RequireExports();
        var buffer = Marshal.AllocHGlobal(state.Length);
        try
        {
            Marshal.Copy(state.ToArray(), BufferConstants.FirstBufferIndex, buffer, state.Length);
            if (!exports.Unserialize(buffer, checked((nuint)state.Length)))
                throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateLoadFailed);
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }
}
