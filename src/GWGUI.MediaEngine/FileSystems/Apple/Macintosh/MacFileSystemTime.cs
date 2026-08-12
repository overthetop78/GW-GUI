namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh;

/// <summary>Convertit les horodatages communs aux systèmes de fichiers Macintosh classiques.</summary>
internal static class MacFileSystemTime
{
    /// <summary>Époque Macintosh, fixée au 1er janvier 1904 à minuit UTC.</summary>
    public static DateTimeOffset Epoch { get; } = new(1904, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>Convertit un nombre de secondes écoulées depuis l'époque Macintosh ; retourne une absence pour zéro ou un dépassement.</summary>
    public static DateTimeOffset? FromSeconds(long seconds)
    {
        if (seconds == 0) return null;
        try { return Epoch.AddSeconds(seconds); }
        catch (ArgumentOutOfRangeException) { return null; }
    }
}
