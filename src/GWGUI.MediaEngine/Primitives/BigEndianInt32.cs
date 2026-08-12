using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Primitives;

/// <summary>Lit des entiers 32 bits big-endian après validation de leur plage.</summary>
public static class BigEndianInt32
{
    /// <summary>Largeur d'un entier 32 bits en octets.</summary>
    public const int Size = sizeof(int);
    /// <summary>Lit un entier signé à l'offset demandé.</summary>
    public static int Read(ReadOnlySpan<byte> data, int offset)
    {
        Validate(data, offset);
        return BinaryPrimitives.ReadInt32BigEndian(data.Slice(offset, Size));
    }
    /// <summary>Lit un entier non signé à l'offset demandé.</summary>
    public static uint ReadUnsigned(ReadOnlySpan<byte> data, int offset)
    {
        Validate(data, offset);
        return BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, Size));
    }
    /// <summary>Vérifie que l'entier tient entièrement dans les données.</summary>
    private static void Validate(ReadOnlySpan<byte> data, int offset)
    {
        if (offset < 0 || offset > data.Length - Size) throw new ArgumentOutOfRangeException(nameof(offset), offset, $"La plage de {Size} octets dépasse les {data.Length} octets disponibles.");
    }
}
