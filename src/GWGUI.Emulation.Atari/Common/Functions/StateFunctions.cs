using System.Buffers.Binary;
using System.Text.Json;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class StateFunctions
{
    internal static bool IsAvailable(ExternalCoreExports exports)
    {
        var size = exports.GetSerializedSize();
        return size > nuint.Zero && size <= CommonConstants.MaximumStateSize;
    }
}

internal static class StateFileFunctions
{
    internal static void Write(string path, SavedStateHeader header, ReadOnlySpan<byte> state)
    {
        SavedStateFunctions.ValidatePayloadSize(state.Length);
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporaryPath = fullPath + StateConstants.TemporaryFileSuffix;
        var headerBytes = JsonSerializer.SerializeToUtf8Bytes(header, StateConstants.JsonOptions);
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(StateConstants.Magic);
                Span<byte> headerLength = stackalloc byte[StateConstants.HeaderLengthSize];
                BinaryPrimitives.WriteInt32LittleEndian(headerLength, headerBytes.Length);
                stream.Write(headerLength);
                stream.Write(headerBytes);
                stream.Write(state);
                stream.Flush(flushToDisk: true);
            }
            if (File.Exists(fullPath)) File.Replace(temporaryPath, fullPath, destinationBackupFileName: null);
            else File.Move(temporaryPath, fullPath);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
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
            throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid,
                StateConstants.TruncatedFileError, error);
        }
        catch (JsonException error)
        {
            throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid,
                StateConstants.InvalidHeaderError, error);
        }
    }

    private static StateFile ReadOnce(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        Span<byte> magic = stackalloc byte[StateConstants.Magic.Length];
        stream.ReadExactly(magic);
        if (!magic.SequenceEqual(StateConstants.Magic))
            throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid,
                StateConstants.InvalidMagicError);
        Span<byte> headerLengthBytes = stackalloc byte[StateConstants.HeaderLengthSize];
        stream.ReadExactly(headerLengthBytes);
        var headerLength = BinaryPrimitives.ReadInt32LittleEndian(headerLengthBytes);
        if (headerLength is <= StateConstants.EmptyLength or > StateConstants.MaximumHeaderLength)
            throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid,
                StateConstants.InvalidHeaderLengthError);
        var headerBytes = GC.AllocateUninitializedArray<byte>(headerLength);
        stream.ReadExactly(headerBytes);
        var header = JsonSerializer.Deserialize<SavedStateHeader>(headerBytes,
            StateConstants.JsonOptions);
        if (header is null || !SavedStateFunctions.IsHeaderValid(header))
            throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid,
                StateConstants.InvalidHeaderError);
        using var payload = new MemoryStream();
        stream.CopyTo(payload);
        var state = payload.ToArray();
        SavedStateFunctions.ValidatePayloadSize(state.Length);
        if (!string.Equals(header.StateSha256, SavedStateFunctions.HashBytes(state),
                StringComparison.OrdinalIgnoreCase))
            throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid,
                StateConstants.CorruptedPayloadError);
        return new StateFile(header, state);
    }
}
