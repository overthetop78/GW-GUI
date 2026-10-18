using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Functions;

internal static class EmulationStateFileFunctions
{
    internal static bool IdentityValuesEqual(IReadOnlyDictionary<string, string>? left, IReadOnlyDictionary<string, string> right)
    {
        if ((left?.Count ?? 0) != right.Count) return false;
        return left is null ? right.Count == 0 : left.All(pair => right.TryGetValue(pair.Key, out var value) && value == pair.Value);
    }

    internal static void WriteFile<THeader>(string path, THeader header, ReadOnlySpan<byte> state, ReadOnlySpan<byte> magic,
        string temporarySuffix, JsonSerializerOptions options, bool useFileReplace)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = fullPath + temporarySuffix;
        var headerBytes = JsonSerializer.SerializeToUtf8Bytes(header, options);
        try
        {
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                Write(stream, magic, headerBytes, state);
            if (useFileReplace && File.Exists(fullPath)) File.Replace(temporary, fullPath, destinationBackupFileName: null);
            else File.Move(temporary, fullPath, overwrite: !useFileReplace);
        }
        finally { ConfigurationFileAccessFunctions.DeleteIfExists(temporary); }
    }

    internal static void Write(FileStream stream, ReadOnlySpan<byte> magic, ReadOnlySpan<byte> header, ReadOnlySpan<byte> state)
    {
        stream.Write(magic);
        Span<byte> length = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(length, header.Length);
        stream.Write(length);
        stream.Write(header);
        stream.Write(state);
        stream.Flush(flushToDisk: true);
    }

    internal static (THeader Header, byte[] State) Read<THeader>(Stream stream, ReadOnlySpan<byte> expectedMagic,
        int maximumHeaderLength, JsonSerializerOptions options, Func<THeader, string> stateHash, Func<Exception> invalid,
        Func<THeader, bool>? validHeader = null, Action<int>? validatePayload = null) where THeader : class
    {
        Span<byte> magic = stackalloc byte[expectedMagic.Length];
        stream.ReadExactly(magic);
        if (!magic.SequenceEqual(expectedMagic)) throw invalid();
        Span<byte> lengthBytes = stackalloc byte[sizeof(int)];
        stream.ReadExactly(lengthBytes);
        var length = BinaryPrimitives.ReadInt32LittleEndian(lengthBytes);
        if (length <= 0 || length > maximumHeaderLength) throw invalid();
        var headerBytes = GC.AllocateUninitializedArray<byte>(length);
        stream.ReadExactly(headerBytes);
        var header = JsonSerializer.Deserialize<THeader>(headerBytes, options);
        if (header is null || validHeader is not null && !validHeader(header)) throw invalid();
        using var payload = new MemoryStream();
        stream.CopyTo(payload);
        var state = payload.ToArray();
        validatePayload?.Invoke(state.Length);
        if (!string.Equals(stateHash(header), HashBytes(state), StringComparison.OrdinalIgnoreCase)) throw invalid();
        return (header, state);
    }

    internal static string HashBytes(ReadOnlySpan<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes));

    internal static string HashPath(string path, string pattern, char separator, int bufferSize, Func<Exception> missing)
    {
        if (File.Exists(path)) return EmulationCoreDiagnosticFunctions.CalculateSha256(path);
        if (!Directory.Exists(path)) throw missing();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> length = stackalloc byte[sizeof(int)];
        foreach (var file in Directory.EnumerateFiles(path, pattern, SearchOption.AllDirectories).Order(StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(path, file).Replace(Path.DirectorySeparatorChar, separator);
            var name = Encoding.UTF8.GetBytes(relative);
            BinaryPrimitives.WriteInt32LittleEndian(length, name.Length);
            hash.AppendData(length);
            hash.AppendData(name);
            using var stream = File.OpenRead(file);
            var buffer = new byte[bufferSize];
            int read;
            while ((read = stream.Read(buffer)) > 0) hash.AppendData(buffer.AsSpan(0, read));
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }
}
