namespace GWGUI.MediaEngine.Representations.Flux;

/// <summary>Recherche des motifs et décode des octets dans un flux de bits reconstruit.</summary>
internal static class FluxBitReader
{
    /// <summary>Vérifie un motif de seize bits à l'offset demandé.</summary>
    /// <param name="stream">Flux de bits à lire.</param>
    /// <param name="offset">Position du premier bit à comparer.</param>
    /// <param name="pattern">Motif de seize bits attendu.</param>
    /// <returns><see langword="true"/> lorsque le motif correspond ; sinon <see langword="false"/>.</returns>
    public static bool Match(FluxBitstream stream, int offset, ushort pattern)
    {
        if (!IsValidRange(stream, offset, FluxDecodingParameters.UshortPatternBitCount)) return false;
        for (var bit = 0; bit < FluxDecodingParameters.UshortPatternBitCount; bit++)
        {
            if (stream.Bits[offset + bit] != ((pattern & (1 << (FluxDecodingParameters.UshortPatternBitCount - 1 - bit))) != 0)) return false;
        }
        return true;
    }

    /// <summary>Vérifie un motif de longueur variable à l'offset demandé.</summary>
    /// <param name="stream">Flux de bits à lire.</param>
    /// <param name="offset">Position du premier bit à comparer.</param>
    /// <param name="pattern">Motif attendu.</param>
    /// <param name="length">Nombre de bits du motif à comparer.</param>
    /// <returns><see langword="true"/> lorsque le motif correspond ; sinon <see langword="false"/>.</returns>
    public static bool Match(FluxBitstream stream, int offset, uint pattern, int length)
    {
        if (length < FluxDecodingParameters.MinimumPatternBitCount || length > FluxDecodingParameters.MaximumUintPatternBitCount || !IsValidRange(stream, offset, length)) return false;
        for (var bit = 0; bit < length; bit++)
        {
            if (stream.Bits[offset + bit] != ((pattern & (1u << (length - 1 - bit))) != 0)) return false;
        }
        return true;
    }

    /// <summary>Vérifie une suite d'octets représentée bit par bit à l'offset demandé.</summary>
    /// <param name="stream">Flux de bits à lire.</param>
    /// <param name="offset">Position du premier bit à comparer.</param>
    /// <param name="pattern">Octets attendus.</param>
    /// <returns><see langword="true"/> lorsque le motif correspond ; sinon <see langword="false"/>.</returns>
    public static bool MatchBytes(FluxBitstream stream, int offset, IReadOnlyList<byte> pattern)
    {
        if (pattern.Count > int.MaxValue / FluxDecodingParameters.BitsPerByte) return false;
        var length = pattern.Count * FluxDecodingParameters.BitsPerByte;
        if (!IsValidRange(stream, offset, length)) return false;
        for (var index = 0; index < pattern.Count; index++)
        {
            for (var bit = 0; bit < FluxDecodingParameters.BitsPerByte; bit++)
            {
                var streamIndex = offset + index * FluxDecodingParameters.BitsPerByte + bit;
                var patternMask = 1 << (FluxDecodingParameters.BitsPerByte - 1 - bit);
                if (stream.Bits[streamIndex] != ((pattern[index] & patternMask) != 0)) return false;
            }
        }
        return true;
    }

    /// <summary>Décode les huit bits de données d'un octet MFM.</summary>
    /// <param name="stream">Flux de bits à lire.</param>
    /// <param name="offset">Position du premier bit d'horloge MFM.</param>
    /// <returns>Octet de données décodé.</returns>
    public static byte DecodeMfmByte(FluxBitstream stream, int offset) { if (!IsValidRange(stream, offset, 16)) return 0; byte value = 0; for (var bit = 0; bit < 8; bit++) if (stream.Bits[offset + bit * 2 + 1]) value |= (byte)(1 << (7 - bit)); return value; }

    /// <summary>Décode huit bits consécutifs en un octet.</summary>
    /// <param name="stream">Flux de bits à lire.</param>
    /// <param name="offset">Position du premier bit.</param>
    /// <returns>Octet décodé.</returns>
    public static byte DecodeByte(FluxBitstream stream, int offset) { if (!IsValidRange(stream, offset, 8)) return 0; byte value = 0; for (var bit = 0; bit < 8; bit++) if (stream.Bits[offset + bit]) value |= (byte)(1 << (7 - bit)); return value; }

    /// <summary>Décode les huit bits de données d'un mot FM de trente-deux bits.</summary>
    /// <param name="stream">Flux de bits à lire.</param>
    /// <param name="offset">Position du premier groupe FM.</param>
    /// <returns>Octet de données décodé.</returns>
    public static byte DecodeFmByte32(FluxBitstream stream, int offset) { if (!IsValidRange(stream, offset, 32)) return 0; byte value = 0; for (var bit = 0; bit < 8; bit++) if (stream.Bits[offset + bit * 4 + 3]) value |= (byte)(1 << (7 - bit)); return value; }

    private static bool IsValidRange(FluxBitstream stream, int offset, int length) => offset >= 0 && length >= 0 && offset <= stream.Bits.Length - length;
}
