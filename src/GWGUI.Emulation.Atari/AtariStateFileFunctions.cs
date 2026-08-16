using System.Buffers.Binary;
using System.Text.Json;

namespace GWGUI.Emulation.Atari;

internal static class AtariStateFileFunctions
{
    internal static void Write(string path, AtariSavedStateHeader header, ReadOnlySpan<byte> state)
    {
        AtariSavedStateFunctions.ValidatePayloadSize(state.Length);
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporaryPath = fullPath + AtariStateConstants.TemporaryFileSuffix;
        var headerBytes = JsonSerializer.SerializeToUtf8Bytes(header, AtariStateConstants.JsonOptions);
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(AtariStateConstants.Magic);
                Span<byte> headerLength = stackalloc byte[AtariStateConstants.HeaderLengthSize];
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

    internal static AtariStateFile Read(string path)
    {
        try
        {
            for (var retry = AtariStateConstants.FirstRetryIndex;
                 retry < AtariStateConstants.ReadRetryCount; retry++)
            {
                try
                {
                    return ReadOnce(path);
                }
                catch (IOException) when (retry + AtariStateConstants.NextRetryCount
                                          < AtariStateConstants.ReadRetryCount)
                {
                    Thread.Sleep(AtariStateConstants.ReadRetryDelayMilliseconds);
                }
            }
            throw new FileNotFoundException(path);
        }
        catch (EndOfStreamException error)
        {
            throw AtariSavedStateFunctions.Invalid(AtariErrorCode.StateInvalid,
                AtariStateConstants.TruncatedFileError, error);
        }
        catch (JsonException error)
        {
            throw AtariSavedStateFunctions.Invalid(AtariErrorCode.StateInvalid,
                AtariStateConstants.InvalidHeaderError, error);
        }
    }

    private static AtariStateFile ReadOnce(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        Span<byte> magic = stackalloc byte[AtariStateConstants.Magic.Length];
        stream.ReadExactly(magic);
        if (!magic.SequenceEqual(AtariStateConstants.Magic))
            throw AtariSavedStateFunctions.Invalid(AtariErrorCode.StateInvalid,
                AtariStateConstants.InvalidMagicError);
        Span<byte> headerLengthBytes = stackalloc byte[AtariStateConstants.HeaderLengthSize];
        stream.ReadExactly(headerLengthBytes);
        var headerLength = BinaryPrimitives.ReadInt32LittleEndian(headerLengthBytes);
        if (headerLength is <= AtariStateConstants.EmptyLength or > AtariStateConstants.MaximumHeaderLength)
            throw AtariSavedStateFunctions.Invalid(AtariErrorCode.StateInvalid,
                AtariStateConstants.InvalidHeaderLengthError);
        var headerBytes = GC.AllocateUninitializedArray<byte>(headerLength);
        stream.ReadExactly(headerBytes);
        var header = JsonSerializer.Deserialize<AtariSavedStateHeader>(headerBytes,
            AtariStateConstants.JsonOptions);
        if (header is null || !AtariSavedStateFunctions.IsHeaderValid(header))
            throw AtariSavedStateFunctions.Invalid(AtariErrorCode.StateInvalid,
                AtariStateConstants.InvalidHeaderError);
        using var payload = new MemoryStream();
        stream.CopyTo(payload);
        var state = payload.ToArray();
        AtariSavedStateFunctions.ValidatePayloadSize(state.Length);
        if (!string.Equals(header.StateSha256, AtariSavedStateFunctions.HashBytes(state),
                StringComparison.OrdinalIgnoreCase))
            throw AtariSavedStateFunctions.Invalid(AtariErrorCode.StateInvalid,
                AtariStateConstants.CorruptedPayloadError);
        return new AtariStateFile(header, state);
    }
}
