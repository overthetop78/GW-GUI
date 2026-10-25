namespace GWGUI.MediaFileSystems.Primitives;

/// <summary>Lit les entiers non signés little-endian stockés sur vingt-quatre bits.</summary>
public static class LittleEndianUInt24
{
    /// <summary>Nombre d'octets d'un entier sur vingt-quatre bits.</summary>
    public const int Size = 3;
    /// <summary>Lit une valeur après validation de la plage.</summary>
    public static int Read(ReadOnlySpan<byte> data, int offset)
    {
        if (offset < 0 || offset > data.Length - Size) throw new ArgumentOutOfRangeException(nameof(offset));
        return Compose(data[offset], data[offset + 1], data[offset + 2]);
    }

    /// <summary>Lit trois octets successifs sans consulter la taille de la source.</summary>
    public static int ReadIndexed(IReadOnlyList<byte> data, int offset)
        => Compose(data[offset], data[offset + 1], data[offset + 2]);

    private static int Compose(byte low, byte middle, byte high)
        => low | middle << Constants.BitConstants.BitsPerByte | high << 16;
}
