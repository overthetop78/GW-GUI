using System.IO;
using System.Buffers.Binary;
using System.Text.Json;

namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class StateFileFunctions
{
    internal static void Write(string path, SavedStateHeader header, ReadOnlySpan<byte> state)
    {
        SavedStateFunctions.ValidatePayloadSize(state.Length);
        EmulationStateFileFunctions.WriteFile(path, header, state, StateConstants.Magic, StateConstants.TemporaryFileSuffix,
            StateConstants.JsonOptions, useFileReplace: true);
    }

    internal static StateFile Read(string path)
    {
        try
        {
            var result = EmulationStateFileFunctions.ReadFile<SavedStateHeader>(path, StateConstants.Magic,
            StateConstants.MaximumHeaderLength, StateConstants.JsonOptions, static header => header.StateSha256,
            static () => SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateInvalid),
            SavedStateFunctions.IsHeaderValid, SavedStateFunctions.ValidatePayloadSize,
                StateConstants.ReadRetryCount, StateConstants.ReadRetryDelayMilliseconds);
            return new StateFile(result.Header, result.State);
        }
        catch (EndOfStreamException error)
        {
            throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateInvalid, error);
        }
        catch (JsonException error)
        {
            throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateInvalid, error);
        }
    }
}
