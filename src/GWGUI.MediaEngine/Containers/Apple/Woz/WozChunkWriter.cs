using System.Buffers.Binary;
using System.Text;

namespace GWGUI.MediaEngine.Containers.Apple.Woz;

/// <summary>Écrit un chunk WOZ après validation de son identifiant et de sa longueur.</summary>
internal static class WozChunkWriter
{
    /// <summary>Écrit l'identifiant, la longueur little-endian et la charge utile d'un chunk.</summary>
    /// <param name="stream">Flux de destination.</param>
    /// <param name="id">Identifiant ASCII de quatre caractères.</param>
    /// <param name="data">Charge utile du chunk.</param>
    public static void Write(Stream stream, string id, ReadOnlySpan<byte> data)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (id.Length != WozLayout.ChunkIdLength || id.Any(character => character > sbyte.MaxValue)) throw WozExceptions.InvalidChunkId(id);
        stream.Write(System.Text.Encoding.ASCII.GetBytes(id));
        Span<byte> length = stackalloc byte[WozLayout.ChunkLengthSize];
        BinaryPrimitives.WriteUInt32LittleEndian(length, checked((uint)data.Length));
        stream.Write(length);
        stream.Write(data);
    }
}
