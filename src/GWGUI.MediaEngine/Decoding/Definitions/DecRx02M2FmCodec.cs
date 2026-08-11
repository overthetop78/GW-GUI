using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Convertit les motifs MFM et M²FM du format RX02.</summary>
internal static class DecRx02M2FmCodec
{
    /// <summary>Remplace dans une séquence MFM les motifs devant être encodés en M²FM.</summary>
    /// <param name="bits">Bits MFM à modifier.</param>
    public static void Encode(List<bool> bits) => Replace(bits, DecRx02Format.NormalM2FmRule, DecRx02Format.EncodedM2FmRule, 1, 2, DecRx02Format.NormalM2FmRule.Count - 3);

    /// <summary>Décode une suite d'octets M²FM.</summary>
    /// <param name="stream">Flux source.</param>
    /// <param name="start">Position du premier bit.</param>
    /// <param name="count">Nombre d'octets.</param>
    /// <returns>Octets décodés, ou valeur nulle si la source est tronquée.</returns>
    public static byte[]? Decode(FluxBitstream stream, int start, int count)
    {
        var requiredBits = count * DecRx02Format.EncodedMfmByteBitCount;
        if (start + requiredBits > stream.Bits.Length) return null;
        var bits = new bool[requiredBits + DecRx02Format.M2FmPhaseBitCount];
        for (var index = 0; index < requiredBits; index++) bits[index + DecRx02Format.M2FmPhaseBitCount] = stream.Bits[start + index];
        Replace(bits, DecRx02Format.EncodedM2FmRule, DecRx02Format.NormalM2FmRule, 0, 1, DecRx02Format.EncodedM2FmRule.Count - 2);
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            for (var bit = 0; bit < BitPrimitives.BitsPerByte; bit++)
            {
                var position = DecRx02Format.M2FmPhaseBitCount + index * DecRx02Format.EncodedMfmByteBitCount + bit * 2;
                if (bits[position] && bits[position + 1]) return null;
                if (!bits[position] && bits[position + 1]) result[index] |= (byte)(1 << (BitPrimitives.BitsPerByte - 1 - bit));
            }
        }
        return result;
    }

    /// <summary>Remplace toutes les occurrences alignées d'un motif par un autre.</summary>
    private static void Replace(IList<bool> bits, IReadOnlyList<bool> source, IReadOnlyList<bool> target, int start, int step, int skip)
    {
        for (var offset = start; offset + source.Count <= bits.Count; offset += step)
        {
            var matches = true;
            for (var index = 0; index < source.Count; index++)
            {
                if (bits[offset + index] == source[index]) continue;
                matches = false;
                break;
            }
            if (!matches || offset % 2 != start % 2) continue;
            for (var index = 0; index < target.Count; index++) bits[offset + index] = target[index];
            offset += skip;
        }
    }
}
