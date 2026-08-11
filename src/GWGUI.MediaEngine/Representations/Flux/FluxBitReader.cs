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
    /// <param name="offset">Position de la première cellule d'horloge parmi les seize cellules MFM alternant horloge et donnée.</param>
    /// <param name="value">Reçoit l'octet formé par les huit cellules de données.</param>
    /// <returns><see langword="true"/> lorsque les seize cellules sont disponibles ; sinon <see langword="false"/>.</returns>
    public static bool TryDecodeMfmByte(FluxBitstream stream, int offset, out byte value)
    {
        value = 0;
        if (!IsValidRange(stream, offset, FluxDecodingParameters.BitsPerByte * FluxDecodingParameters.MfmCellsPerDataBit)) return false;
        for (var bit = 0; bit < FluxDecodingParameters.BitsPerByte; bit++)
        {
            if (stream.Bits[offset + bit * FluxDecodingParameters.MfmCellsPerDataBit + 1]) value |= (byte)(1 << (FluxDecodingParameters.BitsPerByte - 1 - bit));
        }
        return true;
    }

    /// <summary>Décode huit bits consécutifs en un octet.</summary>
    /// <param name="stream">Flux de bits à lire.</param>
    /// <param name="offset">Position du premier bit.</param>
    /// <param name="value">Reçoit l'octet formé par les huit bits consécutifs.</param>
    /// <returns><see langword="true"/> lorsque les huit bits sont disponibles ; sinon <see langword="false"/>.</returns>
    public static bool TryDecodeByte(FluxBitstream stream, int offset, out byte value)
    {
        value = 0;
        if (!IsValidRange(stream, offset, FluxDecodingParameters.BitsPerByte)) return false;
        for (var bit = 0; bit < FluxDecodingParameters.BitsPerByte; bit++)
        {
            if (stream.Bits[offset + bit]) value |= (byte)(1 << (FluxDecodingParameters.BitsPerByte - 1 - bit));
        }
        return true;
    }

    /// <summary>Décode les huit bits de données d'un mot FM de trente-deux bits.</summary>
    /// <param name="stream">Flux de bits à lire.</param>
    /// <param name="offset">Position de la première cellule parmi les huit groupes FM de quatre cellules.</param>
    /// <param name="value">Reçoit l'octet formé par la quatrième cellule de chaque groupe FM.</param>
    /// <returns><see langword="true"/> lorsque les trente-deux cellules sont disponibles ; sinon <see langword="false"/>.</returns>
    public static bool TryDecodeFmByte32(FluxBitstream stream, int offset, out byte value)
    {
        value = 0;
        if (!IsValidRange(stream, offset, FluxDecodingParameters.BitsPerByte * FluxDecodingParameters.FmCellsPerDataBit)) return false;
        for (var bit = 0; bit < FluxDecodingParameters.BitsPerByte; bit++)
        {
            if (stream.Bits[offset + bit * FluxDecodingParameters.FmCellsPerDataBit + FluxDecodingParameters.FmCellsPerDataBit - 1]) value |= (byte)(1 << (FluxDecodingParameters.BitsPerByte - 1 - bit));
        }
        return true;
    }

    /// <summary>Vérifie qu'une plage de bits complète appartient au flux sans addition susceptible de dépasser la capacité d'un entier.</summary>
    /// <param name="stream">Flux de bits contenant la plage.</param>
    /// <param name="offset">Position du premier bit de la plage.</param>
    /// <param name="length">Nombre de bits de la plage.</param>
    /// <returns><see langword="true"/> lorsque la plage est valide ; sinon <see langword="false"/>.</returns>
    private static bool IsValidRange(FluxBitstream stream, int offset, int length) => offset >= 0 && length >= 0 && offset <= stream.Bits.Length - length;
}
