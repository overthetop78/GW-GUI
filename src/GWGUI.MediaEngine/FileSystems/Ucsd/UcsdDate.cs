namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Décode les dates compactes UCSD.</summary>
internal static class UcsdDate
{
    /// <summary>Masque du jour dans une date UCSD.</summary>
    public const int DayMask = 0x1f;
    /// <summary>Masque du mois après décalage.</summary>
    public const int MonthMask = 0x0f;
    /// <summary>Décalage du mois dans une date UCSD.</summary>
    public const int MonthShift = 5;
    /// <summary>Masque de l'année après décalage.</summary>
    public const int YearMask = 0x7f;
    /// <summary>Décalage de l'année dans une date UCSD.</summary>
    public const int YearShift = 9;
    /// <summary>Seuil séparant les années 19xx des années 20xx.</summary>
    public const int CenturyPivot = 70;
    /// <summary>Base des années 19xx.</summary>
    public const int NineteenthCenturyBase = 1900;
    /// <summary>Base des années 20xx.</summary>
    public const int TwentiethCenturyBase = 2000;

    /// <summary>Décode une date ou retourne une absence si elle est nulle ou impossible.</summary>
    public static DateTimeOffset? Decode(ushort value)
    {
        if (value == 0) return null;
        var day = value & DayMask;
        var month = value >> MonthShift & MonthMask;
        var shortYear = value >> YearShift & YearMask;
        var year = shortYear >= CenturyPivot ? NineteenthCenturyBase + shortYear : TwentiethCenturyBase + shortYear;
        try { return new(year, month, day, 0, 0, 0, TimeSpan.Zero); }
        catch (ArgumentOutOfRangeException) { return null; }
    }
}
