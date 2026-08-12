using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Primitives;

/// <summary>Convertit les tailles sectorielles ISO entre nombre d'octets et code exponentiel.</summary>
internal static class SectorSizeCode
{
    /// <summary>Taille du secteur correspondant au code zéro, en octets.</summary>
    public const int MinimumByteCount = 128;
    /// <summary>Taille du secteur correspondant au code sept, en octets.</summary>
    public const int MaximumByteCount = 16_384;

    /// <summary>Obtient le code d'une taille sectorielle, ou zéro lorsqu'elle n'est pas reconnue.</summary>
    /// <param name="sizeBytes">Taille du secteur en octets.</param>
    /// <returns>Code compris entre zéro et sept, ou zéro si la taille n'est pas reconnue.</returns>
    public static byte FromByteCount(int sizeBytes) => TryFromByteCount(sizeBytes, out var code) ? code : TrackEncodingLimits.MinimumSectorSizeCode;

    /// <summary>Tente d'obtenir le code correspondant à une taille sectorielle.</summary>
    /// <param name="sizeBytes">Taille du secteur en octets.</param>
    /// <param name="code">Code compris entre zéro et sept lorsque la conversion réussit.</param>
    /// <returns><see langword="true"/> lorsque la taille est reconnue ; sinon <see langword="false"/>.</returns>
    public static bool TryFromByteCount(int sizeBytes, out byte code)
    {
        for (code = TrackEncodingLimits.MinimumSectorSizeCode; code <= TrackEncodingLimits.MaximumSectorSizeCode; code++)
        {
            if ((MinimumByteCount << code) == sizeBytes) return true;
        }
        code = TrackEncodingLimits.MinimumSectorSizeCode;
        return false;
    }

    /// <summary>Obtient la taille sectorielle correspondant à un code ISO.</summary>
    /// <param name="code">Code compris entre zéro et sept.</param>
    /// <returns>Taille du secteur en octets.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Le code se situe hors des limites admises.</exception>
    public static int ToByteCount(byte code)
    {
        if (code > TrackEncodingLimits.MaximumSectorSizeCode) throw new ArgumentOutOfRangeException(nameof(code), code, $"Sector size code must be between {TrackEncodingLimits.MinimumSectorSizeCode} and {TrackEncodingLimits.MaximumSectorSizeCode}.");
        return MinimumByteCount << code;
    }
}
