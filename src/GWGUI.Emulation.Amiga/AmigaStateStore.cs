using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Amiga;

internal sealed record AmigaSavedStateHeader(int FormatVersion, string Model, string CoreSha256,
    string KickstartSha256, string? MediaSha256, IReadOnlyDictionary<string, string>? Options);

internal static class AmigaStateStore
{
    private static readonly byte[] Magic = "GWAMIGA1"u8.ToArray();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal static void Write(string path, AmigaSavedStateHeader header, ReadOnlySpan<byte> state)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = fullPath + ".tmp";
        var headerBytes = JsonSerializer.SerializeToUtf8Bytes(header, JsonOptions);
        using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            stream.Write(Magic);
            Span<byte> length = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(length, headerBytes.Length);
            stream.Write(length);
            stream.Write(headerBytes);
            stream.Write(state);
            stream.Flush(true);
        }
        File.Move(temporary, fullPath, true);
    }

    internal static (AmigaSavedStateHeader Header, byte[] State) Read(string path)
    {
        using var stream = File.OpenRead(path);
        Span<byte> magic = stackalloc byte[Magic.Length];
        stream.ReadExactly(magic);
        if (!magic.SequenceEqual(Magic)) throw new InvalidDataException("The file is not a GW GUI Amiga state.");
        Span<byte> lengthBytes = stackalloc byte[4];
        stream.ReadExactly(lengthBytes);
        var length = BinaryPrimitives.ReadInt32LittleEndian(lengthBytes);
        if (length is <= 0 or > 1024 * 1024) throw new InvalidDataException("The Amiga state header length is invalid.");
        var headerBytes = new byte[length];
        stream.ReadExactly(headerBytes);
        var header = JsonSerializer.Deserialize<AmigaSavedStateHeader>(headerBytes, JsonOptions)
            ?? throw new InvalidDataException("The Amiga state header is invalid.");
        using var state = new MemoryStream();
        stream.CopyTo(state);
        return (header, state.ToArray());
    }

    internal static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
