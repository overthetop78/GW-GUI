using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Ajoute et contrôle le checksum rotatif puis inverse les bits des enregistrements Heathkit.</summary>
internal static class HeathkitFmCodec
{
    /// <summary>Ajoute le checksum aux octets puis inverse les bits de chaque octet pour l'encodage FM.</summary>
    public static byte[] EncodeRecord(IReadOnlyList<byte> values) => values.Append(RotatingChecksumCalculator.Compute(values)).Select(BitPrimitives.ReverseBits).ToArray();

    /// <summary>Inverse les octets, sépare le checksum final et indique sa validité.</summary>
    public static (byte[] Payload, bool Valid) DecodeRecord(IReadOnlyList<byte> encoded)
    {
        var values = encoded.Select(BitPrimitives.ReverseBits).ToArray();
        var payload = values.SkipLast(1).ToArray();
        return (payload, values[^1] == RotatingChecksumCalculator.Compute(payload));
    }
}
