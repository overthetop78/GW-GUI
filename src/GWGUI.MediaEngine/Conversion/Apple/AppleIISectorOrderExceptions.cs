namespace GWGUI.MediaEngine.Conversion.Apple;

/// <summary>Construit les erreurs de conversion des ordres sectoriels Apple II.</summary>
internal static class AppleIISectorOrderExceptions
{
    /// <summary>Crée l'erreur signalant une longueur qui ne contient pas un nombre entier de pistes.</summary>
    public static InvalidDataException InvalidLength(int actualLength, int trackSize) => new($"Apple II image contains {actualLength} bytes; its length must be a multiple of {trackSize} bytes.");
    /// <summary>Crée l'erreur signalant un numéro de secteur extérieur à l'ordre sectoriel Apple II.</summary>
    public static ArgumentOutOfRangeException InvalidSector(int observed, int sectorCount) => new(nameof(observed), observed, $"Apple II sector must be between 0 and {sectorCount - 1}.");
}
