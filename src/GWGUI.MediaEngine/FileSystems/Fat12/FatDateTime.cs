namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Décode les champs de date et heure FAT.</summary>
public static class FatDateTime
{
    /// <summary>Année de base du calendrier FAT.</summary>
    public const int BaseYear = 1980;
    /// <summary>Décalage de l'année dans la date.</summary>
    public const int YearShift = 9;
    /// <summary>Décalage du mois dans la date.</summary>
    public const int MonthShift = 5;
    /// <summary>Masque du mois.</summary>
    public const int MonthMask = 0x0f;
    /// <summary>Masque du jour.</summary>
    public const int DayMask = 0x1f;
    /// <summary>Décalage de l'heure.</summary>
    public const int HourShift = 11;
    /// <summary>Décalage des minutes.</summary>
    public const int MinuteShift = 5;
    /// <summary>Masque des minutes.</summary>
    public const int MinuteMask = 0x3f;
    /// <summary>Masque des secondes divisées par deux.</summary>
    public const int SecondMask = 0x1f;
    /// <summary>Multiplicateur des secondes stockées.</summary>
    public const int SecondMultiplier = 2;

    /// <summary>Retourne une date absente lorsque les champs ne décrivent pas une date valide.</summary>
    public static DateTimeOffset? Decode(ushort date, ushort time)
    {
        try
        {
            var year = BaseYear + (date >> YearShift);
            var month = date >> MonthShift & MonthMask;
            var day = date & DayMask;
            if (month == 0 || day == 0) return null;
            return new DateTimeOffset(year, month, day, time >> HourShift, time >> MinuteShift & MinuteMask, (time & SecondMask) * SecondMultiplier, TimeSpan.Zero);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
