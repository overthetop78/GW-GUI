using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Amiga.Common.Services;


internal static class StateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal static void Write(string path, SavedStateHeader header, ReadOnlySpan<byte> state)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = fullPath + StateStoreConstants.Tmp;
        var headerBytes = JsonSerializer.SerializeToUtf8Bytes(header, JsonOptions);
        try
        {
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(StateStoreConstants.Magic);
                Span<byte> length = stackalloc byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(length, headerBytes.Length);
                stream.Write(length);
                stream.Write(headerBytes);
                stream.Write(state);
                stream.Flush(true);
            }
            File.Move(temporary, fullPath, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    internal static (SavedStateHeader Header, byte[] State) Read(string path)
    {
        using var stream = File.OpenRead(path);
        Span<byte> magic = stackalloc byte[StateStoreConstants.Magic.Length];
        stream.ReadExactly(magic);
        if (!magic.SequenceEqual(StateStoreConstants.Magic)) throw MachineExceptions.SavedStateInvalid();
        Span<byte> lengthBytes = stackalloc byte[4];
        stream.ReadExactly(lengthBytes);
        var length = BinaryPrimitives.ReadInt32LittleEndian(lengthBytes);
        if (length is <= 0 or > StateStoreConstants.MaximumHeaderLength)
            throw MachineExceptions.SavedStateInvalid();
        var headerBytes = new byte[length];
        stream.ReadExactly(headerBytes);
        var header = JsonSerializer.Deserialize<SavedStateHeader>(headerBytes, JsonOptions)
            ?? throw MachineExceptions.SavedStateInvalid();
        using var state = new MemoryStream();
        stream.CopyTo(state);
        var stateBytes = state.ToArray();
        if (header.StateSha256 is { Length: > 0 } expected && !HashBytes(stateBytes).Equals(expected, StringComparison.OrdinalIgnoreCase))
            throw MachineExceptions.SavedStateInvalid();
        return (header, stateBytes);
    }

    internal static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    internal static string HashPath(string path)
    {
        if (File.Exists(path)) return HashFile(path);
        if (!Directory.Exists(path)) throw MachineExceptions.MediaNotFound();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> length = stackalloc byte[sizeof(int)];
        foreach (var file in Directory.EnumerateFiles(path, StateStoreConstants.Value, SearchOption.AllDirectories)
                     .Order(StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(path, file).Replace(Path.DirectorySeparatorChar, '/');
            var name = Encoding.UTF8.GetBytes(relative);
            BinaryPrimitives.WriteInt32LittleEndian(length, name.Length);
            hash.AppendData(length);
            hash.AppendData(name);
            using var stream = File.OpenRead(file);
            var buffer = new byte[64 * 1024];
            int read;
            while ((read = stream.Read(buffer)) > 0) hash.AppendData(buffer.AsSpan(0, read));
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    internal static string HashBytes(ReadOnlySpan<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes));
}
