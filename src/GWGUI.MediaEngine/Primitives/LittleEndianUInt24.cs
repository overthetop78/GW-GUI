namespace GWGUI.MediaEngine.Primitives;

/// <summary>Lit les entiers non signés little-endian stockés sur vingt-quatre bits.</summary>
public static class LittleEndianUInt24
{
    /// <summary>Nombre d'octets d'un entier sur vingt-quatre bits.</summary>
    public const int Size = 3;
    /// <summary>Lit une valeur après validation de la plage.</summary>
    public static int Read(ReadOnlySpan<byte> data, int offset)
    {
        if (offset < 0 || offset > data.Length - Size) throw new ArgumentOutOfRangeException(nameof(offset));
        return data[offset] | data[offset + 1] << BitPrimitives.BitsPerByte | data[offset + 2] << 16;
    }
}
