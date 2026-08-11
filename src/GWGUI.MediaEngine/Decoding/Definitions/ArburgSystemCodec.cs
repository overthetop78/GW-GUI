namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Encode et décode la représentation binaire variable des blocs système Arburg.</summary>
internal static class ArburgSystemCodec
{
    /// <summary>Décode un nombre déterminé d'octets système depuis un flux binaire.</summary>
    /// <param name="stream">Flux binaire source.</param>
    /// <param name="start">Position du premier bit à décoder.</param>
    /// <param name="count">Nombre d'octets attendus.</param>
    /// <returns>Octets décodés et position finale, ou <see langword="null"/> lorsque la séquence est invalide ou tronquée.</returns>
    public static (byte[] Bytes, int EndOffset)? Decode(FluxBitstream stream, int start, int count)
    {
        var result = new byte[count];
        var offset = start;
        for (var index = 0; index < count; index++)
        {
            byte value = 0;
            for (var bit = 0; bit < Primitives.BitPrimitives.BitsPerByte; bit++)
            {
                if (offset + ArburgFormat.SystemZeroEncodedBitCount > stream.Bits.Length || stream.Bits[offset] != ArburgFormat.SystemPrefixBit) return null;
                if (stream.Bits[offset + 1] == ArburgFormat.SystemZeroSecondBit) offset += ArburgFormat.SystemZeroEncodedBitCount;
                else
                {
                    if (offset + ArburgFormat.SystemOneEncodedBitCount > stream.Bits.Length || stream.Bits[offset + 1] != ArburgFormat.SystemOneSecondBit || stream.Bits[offset + 2] != ArburgFormat.SystemOneThirdBit) return null;
                    value |= (byte)(1 << bit);
                    offset += ArburgFormat.SystemOneEncodedBitCount;
                }
            }
            result[index] = value;
        }
        return (result, offset);
    }

    /// <summary>Encode des octets avec la représentation système Arburg.</summary>
    /// <param name="values">Octets à encoder.</param>
    /// <returns>Bits encodés.</returns>
    public static IReadOnlyList<bool> Encode(IEnumerable<byte> values)
    {
        var bits = new List<bool>();
        foreach (var value in values)
            for (var bit = 0; bit < Primitives.BitPrimitives.BitsPerByte; bit++)
            {
                bits.Add(ArburgFormat.SystemPrefixBit);
                var set = ((value >> bit) & 1) != 0;
                bits.Add(set ? ArburgFormat.SystemOneSecondBit : ArburgFormat.SystemZeroSecondBit);
                if (set) bits.Add(ArburgFormat.SystemOneThirdBit);
            }
        return bits;
    }
}
