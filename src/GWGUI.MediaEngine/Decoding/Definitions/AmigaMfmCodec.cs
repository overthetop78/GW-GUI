namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Encode, décode et contrôle les blocs odd/even du format MFM Amiga.</summary>
internal static class AmigaMfmCodec
{
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

    public static (byte High, byte Low) CalculateParity(IReadOnlyList<byte> encoded, int offset, int count)
    {
        byte high = 0;
        byte low = 0;
        for (var index = 0; index < count; index += 4)
        {
            high ^= (byte)(encoded[offset + index] ^ encoded[offset + index + 2]);
            low ^= (byte)(encoded[offset + index + 1] ^ encoded[offset + index + 3]);
        }
        return (high, low);
    }

    public static (byte High, byte Low) CalculateSplitParity(IReadOnlyList<byte> encoded, int offset, int count)
    {
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

    private static byte Nibble(byte value, bool odd)
    {
        byte result = 0;
        var first = odd ? 7 : 6;
        for (var index = 0; index < AmigaMfmFormat.NibbleBitCount; index++) result |= (byte)(((value >> (first - index * 2)) & 1) << (3 - index));
        return result;
    }
}
