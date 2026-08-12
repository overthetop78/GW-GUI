namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Encode, décode et contrôle les blocs odd/even du format MFM Amiga.</summary>
internal static class AmigaMfmCodec
{
    /// <summary>Décode un bloc Amiga dont les bits impairs et pairs sont stockés séparément.</summary>
    /// <param name="encoded">Octets encodés, première moitié impaire puis seconde moitié paire.</param>
    /// <returns>Octets reconstitués dans leur ordre logique.</returns>
    public static byte[] DecodeOddEven(IReadOnlyList<byte> encoded)
    {
        var result = new byte[encoded.Count];
        var half = encoded.Count / 2;
        for (var index = 0; index < half; index++)
        {
            var odd = encoded[index];
            var even = encoded[index + half];
            result[index * 2] = Interleave((byte)(odd >> AmigaMfmFormat.NibbleBitCount), (byte)(even >> AmigaMfmFormat.NibbleBitCount));
            result[index * 2 + 1] = Interleave((byte)(odd & 15), (byte)(even & 15));
        }
        return result;
    }

    /// <summary>Encode des octets en séparant leurs bits impairs et pairs selon le format Amiga.</summary>
    /// <param name="values">Octets à encoder ; leur nombre doit être pair.</param>
    /// <returns>Bloc odd/even, moitié impaire suivie de la moitié paire.</returns>
    public static byte[] EncodeOddEven(IReadOnlyList<byte> values)
    {
        if ((values.Count & 1) != 0) throw AmigaMfmFormat.OddEncodedByteCount(values.Count);
        var odd = new List<byte>();
        var even = new List<byte>();
        for (var index = 0; index < values.Count; index += 2)
        {
            odd.Add((byte)(Nibble(values[index], true) << AmigaMfmFormat.NibbleBitCount | Nibble(values[index + 1], true)));
            even.Add((byte)(Nibble(values[index], false) << AmigaMfmFormat.NibbleBitCount | Nibble(values[index + 1], false)));
        }
        return odd.Concat(even).ToArray();
    }

    /// <summary>Calcule les deux octets de parité entrelacée d'une plage encodée.</summary>
    /// <param name="encoded">Bloc encodé contenant la plage.</param>
    /// <param name="offset">Position du premier octet couvert.</param>
    /// <param name="count">Nombre d'octets couverts.</param>
    /// <returns>Octets haut et bas de la parité calculée.</returns>
    public static (byte High, byte Low) CalculateParity(IReadOnlyList<byte> encoded, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset + count > encoded.Count || count % AmigaMfmFormat.InfoByteCount != 0) throw AmigaMfmFormat.InvalidParityRange(offset, count, encoded.Count);
        byte high = 0;
        byte low = 0;
        for (var index = 0; index < count; index += 4)
        {
            high ^= (byte)(encoded[offset + index] ^ encoded[offset + index + 2]);
            low ^= (byte)(encoded[offset + index + 1] ^ encoded[offset + index + 3]);
        }
        return (high, low);
    }

    /// <summary>Calcule les deux octets de parité d'un bloc séparé en moitiés impaire et paire.</summary>
    /// <param name="encoded">Bloc encodé contenant la plage.</param>
    /// <param name="offset">Position du premier octet couvert.</param>
    /// <param name="count">Nombre d'octets couverts, répartis également entre les deux moitiés.</param>
    /// <returns>Octets haut et bas de la parité calculée.</returns>
    public static (byte High, byte Low) CalculateSplitParity(IReadOnlyList<byte> encoded, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset + count > encoded.Count || count % AmigaMfmFormat.InfoByteCount != 0) throw AmigaMfmFormat.InvalidParityRange(offset, count, encoded.Count);
        byte high = 0;
        byte low = 0;
        var half = count / 2;
        for (var index = 0; index < half; index += 2)
        {
            high ^= (byte)(encoded[offset + index] ^ encoded[offset + half + index]);
            low ^= (byte)(encoded[offset + index + 1] ^ encoded[offset + half + index + 1]);
        }
        return (high, low);
    }

    /// <summary>Entrelace deux quartets contenant respectivement les bits impairs et pairs d'un octet.</summary>
    /// <param name="odd">Quartet contenant les bits impairs.</param>
    /// <param name="even">Quartet contenant les bits pairs.</param>
    /// <returns>Octet reconstitué.</returns>
    private static byte Interleave(byte odd, byte even)
    {
        byte value = 0;
        for (var index = 0; index < AmigaMfmFormat.NibbleBitCount; index++)
        {
            value |= (byte)(((odd >> (3 - index)) & 1) << (7 - index * 2));
            value |= (byte)(((even >> (3 - index)) & 1) << (6 - index * 2));
        }
        return value;
    }

    /// <summary>Extrait les bits impairs ou pairs d'un octet sous la forme d'un quartet.</summary>
    /// <param name="value">Octet source.</param>
    /// <param name="odd"><see langword="true"/> pour extraire les bits impairs, sinon les bits pairs.</param>
    /// <returns>Quartet extrait dans les quatre bits de poids faible.</returns>
    private static byte Nibble(byte value, bool odd)
    {
        byte result = 0;
        var first = odd ? 7 : 6;
        for (var index = 0; index < AmigaMfmFormat.NibbleBitCount; index++) result |= (byte)(((value >> (first - index * 2)) & 1) << (3 - index));
        return result;
    }
}
