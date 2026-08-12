using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Centralise les transformations binaires du format HP MMFM.</summary>
internal static class HpMmfmCodec
{
    /// <summary>Décode des octets MFM consécutifs.</summary>
    public static byte[]? DecodeBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * HpMmfmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }

    /// <summary>Transforme la charge utile logique vers l'ordre encodé HP.</summary>
    public static byte[] EncodePayload(IReadOnlyList<byte> payload)
    {
        if ((payload.Count & 1) != 0) throw new ArgumentException($"HP MMFM pair permutation requires an even byte count; received {payload.Count}.", nameof(payload));
        var result = payload.ToArray();
        SwapPairs(result);
        for (var index = 0; index < result.Length; index++) result[index] = BitPrimitives.ReverseBits(result[index]);
        return result;
    }

    /// <summary>Restaure la charge utile logique depuis l'ordre encodé HP.</summary>
    public static byte[] DecodePayload(IReadOnlyList<byte> payload)
    {
        if ((payload.Count & 1) != 0) throw new ArgumentException($"HP MMFM pair permutation requires an even byte count; received {payload.Count}.", nameof(payload));
        var result = payload.Select(BitPrimitives.ReverseBits).ToArray();
        SwapPairs(result);
        return result;
    }

    /// <summary>Échange en place chaque paire d'octets.</summary>
    private static void SwapPairs(byte[] values)
    {
        for (var index = 0; index < values.Length; index += 2) (values[index], values[index + 1]) = (values[index + 1], values[index]);
    }
}
