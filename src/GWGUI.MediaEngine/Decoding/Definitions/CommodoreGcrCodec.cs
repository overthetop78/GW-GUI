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
    public static IReadOnlyList<byte> EncodingTable { get; } = Array.AsReadOnly<byte>(
    [
        0x0a, 0x0b, 0x12, 0x13, 0x0e, 0x0f, 0x16, 0x17,
        0x09, 0x19, 0x1a, 0x1b, 0x0d, 0x1d, 0x1e, 0x15
    ]);
    /// <summary>Table inverse construite depuis la table d'encodage.</summary>
    public static IReadOnlyDictionary<int, int> DecodingTable { get; } = EncodingTable.Select((value, index) => ((int)value, index)).ToDictionary(item => item.Item1, item => item.index);

    /// <summary>Décode un symbole GCR de cinq bits.</summary>
    /// <param name="bits">Bits source.</param><param name="offset">Position du premier bit.</param><param name="stride">Écart entre deux cellules utiles.</param><param name="value">Demi-octet décodé.</param><returns><see langword="true"/> si le symbole est complet et valide.</returns>
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
    /// <param name="bits">Bits source.</param><param name="offset">Position du premier symbole.</param><param name="value">Octet décodé.</param><returns><see langword="true"/> si les deux symboles sont valides.</returns>
    public static bool TryDecodeByte(IReadOnlyList<bool> bits, int offset, out byte value)
        => TryDecodeByte(bits, offset, 1, out value);

    /// <summary>Décode un octet depuis deux symboles GCR séparés par le pas indiqué.</summary>
    public static bool TryDecodeByte(IReadOnlyList<bool> bits, int offset, int stride, out byte value)
    {
        value = 0;
        if (!TryDecodeNibble(bits, offset, stride, out var high) || !TryDecodeNibble(bits, offset + EncodedNibbleBitCount * stride, stride, out var low)) return false;
        value = (byte)((high << 4) | low);
        return true;
    }

    /// <summary>Décode une suite d'octets GCR consécutifs.</summary>
    /// <param name="bits">Bits source.</param><param name="offset">Position de départ.</param><param name="count">Nombre d'octets attendu.</param><returns>Octets décodés, ou <see langword="null"/> si un symbole est invalide ou tronqué.</returns>
    public static byte[]? TryDecodeBytes(IReadOnlyList<bool> bits, int offset, int count)
        => TryDecodeBytes(bits, offset, count, 1, out _);

    /// <summary>Décode des octets GCR en utilisant le pas de cellules indiqué.</summary>
    public static byte[]? TryDecodeBytes(IReadOnlyList<bool> bits, int offset, int count, int stride, out int endOffset)
    {
        endOffset = offset + count * EncodedByteBitCount * stride;
        if (endOffset > bits.Count) return null;
        var result = new byte[count];
        for (var index = 0; index < count; index++)
            if (!TryDecodeByte(bits, offset + index * EncodedByteBitCount * stride, stride, out result[index])) return null;
        return result;
    }

    /// <summary>Encode une suite d'octets en cellules GCR consécutives.</summary>
    /// <param name="values">Octets à encoder.</param><returns>Cellules GCR produites.</returns>
    public static IReadOnlyList<bool> Encode(IEnumerable<byte> values)
    {
        var bits = new List<bool>();
        foreach (var value in values)
            foreach (var nibble in new[] { value >> 4, value & NibbleMask })
                for (var bit = EncodedNibbleBitCount - 1; bit >= 0; bit--)
                    bits.Add((EncodingTable[nibble] & (1 << bit)) != 0);
        return bits;
    }

    /// <summary>Insère des cellules GCR dans une cible en respectant le pas indiqué.</summary>
    public static void Write(IList<bool> target, int offset, IEnumerable<byte> values, int stride)
    {
        var encoded = Encode(values);
        var required = offset + encoded.Count * stride;
        while (target.Count < required) target.Add(false);
        for (var index = 0; index < encoded.Count; index++) target[offset + index * stride] = encoded[index];
    }
}
