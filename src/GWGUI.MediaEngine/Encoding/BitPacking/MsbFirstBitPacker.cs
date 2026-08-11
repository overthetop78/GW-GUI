using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding.BitPacking;

/// <summary>Convertit un flux d'octets en bits ordonnés du poids fort vers le poids faible.</summary>
internal static class MsbFirstBitPacker
{
    /// <summary>Calcule le nombre d'octets requis pour contenir un nombre positif de bits.</summary>
    /// <param name="bitCount">Nombre de bits à stocker.</param>
    /// <returns>Nombre minimal d'octets requis.</returns>
    public static int RequiredByteCount(int bitCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bitCount);
        return checked((bitCount + BitPrimitives.BitsPerByte - 1) / BitPrimitives.BitsPerByte);
    }

    /// <summary>Extrait exactement le nombre demandé de bits depuis les octets fournis.</summary>
    /// <param name="bytes">Octets à convertir, lus du bit de poids fort vers le bit de poids faible.</param>
    /// <param name="bitCount">Nombre exact de bits à extraire.</param>
    /// <returns>Bits extraits dans leur ordre d'origine.</returns>
    public static bool[] Unpack(ReadOnlySpan<byte> bytes, int bitCount)
    {
        var byteCount = RequiredByteCount(bitCount);
        if (byteCount > bytes.Length) throw new ArgumentOutOfRangeException(nameof(bitCount), "The requested bit count exceeds the available bytes.");
        var bits = new bool[bitCount];
        for (var bit = 0; bit < bitCount; bit++) bits[bit] = (bytes[bit / BitPrimitives.BitsPerByte] & 1 << (BitPrimitives.BitsPerByte - 1 - bit % BitPrimitives.BitsPerByte)) != 0;
        return bits;
    }
}
