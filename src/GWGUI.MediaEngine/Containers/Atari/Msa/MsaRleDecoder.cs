using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.Atari.Msa;

/// <summary>Décompresse les pistes MSA encodées par répétitions RLE.</summary>
internal static class MsaRleDecoder
{
    /// <summary>Décompresse une piste en exigeant exactement la longueur attendue.</summary>
    /// <param name="packed">Données compressées de la piste.</param>
    /// <param name="expected">Longueur décompressée attendue, en octets.</param>
    /// <param name="cylinder">Cylindre utilisé dans les diagnostics.</param>
    /// <param name="head">Face utilisée dans les diagnostics.</param>
    /// <returns>La piste décompressée.</returns>
    /// <exception cref="InvalidDataException">Une séquence est tronquée, dépasse la piste ou produit une longueur incorrecte.</exception>
    public static byte[] Unpack(ReadOnlySpan<byte> packed, int expected, int cylinder, int head)
    {
        var output = new byte[expected];
        var input = 0;
        var written = 0;
        while (input < packed.Length && written < output.Length)
        {
            if (packed[input] != MsaFormat.RleMarker)
            {
                output[written++] = packed[input++];
                continue;
            }
            if (input + MsaLayout.RleSequenceSize > packed.Length) throw MsaExceptions.TruncatedRun(cylinder, head, input, packed.Length);
            var value = packed[input + MsaLayout.RleValueOffset];
            var count = BinaryPrimitives.ReadUInt16BigEndian(packed[(input + MsaLayout.RleCountOffset)..]);
            input += MsaLayout.RleSequenceSize;
            if (count == 0 || written + count > output.Length) throw MsaExceptions.InvalidRun(cylinder, head, input - MsaLayout.RleSequenceSize, count, written, expected);
            output.AsSpan(written, count).Fill(value);
            written += count;
        }
        if (input != packed.Length || written != expected) throw MsaExceptions.InvalidUnpackedLength(cylinder, head, input, packed.Length, written, expected);
        return output;
    }
}
