namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Encode et décode les symboles GCR 4-vers-5 communs aux formats Commodore et Victor.</summary>
internal static class CommodoreGcrCodec
{
    /// <summary>Nombre de bits composant un symbole GCR.</summary>
    public const int EncodedNibbleBitCount = 5;
    /// <summary>Nombre de bits composant deux symboles représentant un octet.</summary>
    public const int EncodedByteBitCount = EncodedNibbleBitCount * 2;
    /// <summary>Masque isolant un demi-octet.</summary>
    public const int NibbleMask = 0x0f;
    /// <summary>Table unique des seize symboles GCR.</summary>
    public static IReadOnlyList<int> EncodingTable { get; } = Array.AsReadOnly<int>(
    [
        0x0a, 0x0b, 0x12, 0x13, 0x0e, 0x0f, 0x16, 0x17,
        0x09, 0x19, 0x1a, 0x1b, 0x0d, 0x1d, 0x1e, 0x15
    ]);
    /// <summary>Table inverse construite depuis la table d'encodage.</summary>
    public static IReadOnlyDictionary<int, int> DecodingTable { get; } = EncodingTable.Select((value, index) => (value, index)).ToDictionary(item => item.value, item => item.index);

    /// <summary>Décode un symbole GCR de cinq bits.</summary>
    public static bool TryDecodeNibble(IReadOnlyList<bool> bits, int offset, int stride, out byte value)
    {
        var code = 0;
        value = 0;
        for (var bit = 0; bit < EncodedNibbleBitCount; bit++)
        {
            var position = offset + bit * stride;
            if (position >= bits.Count) return false;
            code = (code << 1) | (bits[position] ? 1 : 0);
        }
        if (!DecodingTable.TryGetValue(code, out var decoded)) return false;
        value = (byte)decoded;
        return true;
    }

    /// <summary>Décode un octet depuis deux symboles GCR consécutifs.</summary>
    public static bool TryDecodeByte(IReadOnlyList<bool> bits, int offset, out byte value)
    {
        value = 0;
        if (!TryDecodeNibble(bits, offset, 1, out var high) || !TryDecodeNibble(bits, offset + EncodedNibbleBitCount, 1, out var low)) return false;
        value = (byte)((high << 4) | low);
        return true;
    }

    /// <summary>Décode une suite d'octets GCR consécutifs.</summary>
    public static byte[]? TryDecodeBytes(IReadOnlyList<bool> bits, int offset, int count)
    {
        if (offset + count * EncodedByteBitCount > bits.Count) return null;
        var result = new byte[count];
        for (var index = 0; index < count; index++)
            if (!TryDecodeByte(bits, offset + index * EncodedByteBitCount, out result[index])) return null;
        return result;
    }

    /// <summary>Encode une suite d'octets en cellules GCR consécutives.</summary>
    public static IReadOnlyList<bool> Encode(IEnumerable<byte> values)
    {
        var bits = new List<bool>();
        foreach (var value in values)
            foreach (var nibble in new[] { value >> 4, value & NibbleMask })
                for (var bit = EncodedNibbleBitCount - 1; bit >= 0; bit--)
                    bits.Add((EncodingTable[nibble] & (1 << bit)) != 0);
        return bits;
    }
}
