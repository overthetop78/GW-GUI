namespace GWGUI.MediaEngine.FileSystems.Acorn;

/// <summary>Décode les horodatages RISC OS conservés dans les champs load et execute.</summary>
public static class AcornFileSystemTime
{
    /// <summary>Époque RISC OS exprimée en UTC.</summary>
    public static DateTimeOffset Epoch { get; } = new(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
    /// <summary>Masque signalant la présence d'un horodatage.</summary>
    public const uint TimestampMarkerMask = 0xfff00000;
    /// <summary>Valeur signalant la présence d'un horodatage.</summary>
    public const uint TimestampMarker = TimestampMarkerMask;
    /// <summary>Masque des bits hauts des centisecondes.</summary>
    public const uint HighCentisecondsMask = 0xff;
    /// <summary>Nombre de millisecondes par centiseconde.</summary>
    public const double MillisecondsPerCentisecond = 10d;

    /// <summary>Indique si le champ load contient un horodatage RISC OS.</summary>
    public static bool HasTimestamp(uint load) => (load & TimestampMarkerMask) == TimestampMarker;

    /// <summary>Décode un horodatage RISC OS ou retourne une valeur absente.</summary>
    public static DateTimeOffset? Decode(uint load, uint execute)
    {
        if (!HasTimestamp(load)) return null;
        var centiseconds = ((ulong)(load & HighCentisecondsMask) << 32) | execute;
        try { return Epoch.AddMilliseconds(centiseconds * MillisecondsPerCentisecond); }
        catch (ArgumentOutOfRangeException) { return null; }
    }
}
