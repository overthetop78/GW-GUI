namespace GWGUI.MediaEngine.Primitives;

/// <summary>Convertit les tailles sectorielles ISO entre nombre d'octets et code exponentiel.</summary>
internal static class SectorSizeCode
{
    /// <summary>Taille du secteur correspondant au code zéro, en octets.</summary>
    public const int MinimumByteCount = 128;
    /// <summary>Taille du secteur correspondant au code sept, en octets.</summary>
    public const int MaximumByteCount = 16_384;
    /// <summary>Plus petit code de taille pris en charge.</summary>
    public const byte MinimumCode = 0;
    /// <summary>Plus grand code de taille pris en charge.</summary>
    public const byte MaximumCode = 7;

    /// <summary>Obtient le code d'une taille sectorielle, ou zéro lorsqu'elle n'est pas reconnue.</summary>
    /// <param name="sizeBytes">Taille du secteur en octets.</param>
    /// <returns>Code compris entre zéro et sept, ou zéro si la taille n'est pas reconnue.</returns>
    /// <exception cref="ArgumentException">La taille ne correspond à aucun code pris en charge.</exception>
    public static byte FromByteCount(int sizeBytes) => TryFromByteCount(sizeBytes, out var code) ? code : throw UnsupportedByteCount(sizeBytes);

    /// <summary>Tente d'obtenir le code correspondant à une taille sectorielle.</summary>
    /// <param name="sizeBytes">Taille du secteur en octets.</param>
    /// <param name="code">Code compris entre zéro et sept lorsque la conversion réussit.</param>
    /// <returns><see langword="true"/> lorsque la taille est reconnue ; sinon <see langword="false"/>.</returns>
    public static bool TryFromByteCount(int sizeBytes, out byte code)
    {
        for (code = MinimumCode; code <= MaximumCode; code++)
        {
            if ((MinimumByteCount << code) == sizeBytes) return true;
        }
        code = MinimumCode;
        return false;
    }

    /// <summary>Obtient la taille sectorielle correspondant à un code ISO.</summary>
    /// <param name="code">Code compris entre zéro et sept.</param>
    /// <returns>Taille du secteur en octets.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Le code se situe hors des limites admises.</exception>
    public static int ToByteCount(byte code)
    {
        if (code > MaximumCode) throw UnsupportedCode(code);
        return MinimumByteCount << code;
    }

    /// <summary>Crée l'erreur signalant une taille sans code correspondant.</summary>
    private static ArgumentException UnsupportedByteCount(int sizeBytes) => new($"Unsupported sector size: {sizeBytes} bytes.", nameof(sizeBytes));

    /// <summary>Crée l'erreur signalant un code situé hors des limites prises en charge.</summary>
    private static ArgumentOutOfRangeException UnsupportedCode(byte code) => new(nameof(code), code, $"Sector size code must be between {MinimumCode} and {MaximumCode}.");
}
