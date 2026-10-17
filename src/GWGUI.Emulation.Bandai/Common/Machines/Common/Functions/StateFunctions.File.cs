using System.IO;
using System.Buffers.Binary;
using System.Text.Json;

namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

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
            for (var retry = StateConstants.FirstRetryIndex;
                 retry < StateConstants.ReadRetryCount; retry++)
            {
                try
                {
                    return ReadOnce(path);
                }
                catch (IOException) when (retry + StateConstants.NextRetryCount
                                          < StateConstants.ReadRetryCount)
                {
                    Thread.Sleep(StateConstants.ReadRetryDelayMilliseconds);
                }
            }
            throw new FileNotFoundException(path);
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

    private static StateFile ReadOnce(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var result = EmulationStateFileFunctions.Read<SavedStateHeader>(stream, StateConstants.Magic,
            StateConstants.MaximumHeaderLength, StateConstants.JsonOptions, static header => header.StateSha256,
            static () => SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateInvalid),
            SavedStateFunctions.IsHeaderValid, SavedStateFunctions.ValidatePayloadSize);
        return new StateFile(result.Header, result.State);
    }
}
