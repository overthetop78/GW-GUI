namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Décode les dates compactes RT-11.</summary>
public static class Rt11Date
{
    /// <summary>Définit la valeur RT-11 nommée <c>BaseYear</c>.</summary>
    public const int BaseYear = 1972;
    /// <summary>Définit la valeur RT-11 nommée <c>DayMask</c>.</summary>
    public const int DayMask = 0x1f;
    /// <summary>Définit la valeur RT-11 nommée <c>MonthMask</c>.</summary>
    public const int MonthMask = 0x0f;
    /// <summary>Définit la valeur RT-11 nommée <c>MonthShift</c>.</summary>
    public const int MonthShift = 5;
    /// <summary>Définit la valeur RT-11 nommée <c>YearMask</c>.</summary>
    public const int YearMask = 0x1f;
    /// <summary>Définit la valeur RT-11 nommée <c>YearShift</c>.</summary>
    public const int YearShift = 9;
    /// <summary>Définit la valeur RT-11 nommée <c>AgeMask</c>.</summary>
    public const int AgeMask = 0x03;
    /// <summary>Définit la valeur RT-11 nommée <c>AgeShift</c>.</summary>
    public const int AgeShift = 14;
    /// <summary>Définit la valeur RT-11 nommée <c>YearsPerAge</c>.</summary>
    public const int YearsPerAge = 32;

    /// <summary>Décode la date, ou retourne une absence si elle est nulle ou impossible.</summary>
    public static DateTimeOffset? Decode(ushort word)
    {
        if (word == 0) return null;
        var day = word & DayMask;
        var month = word >> MonthShift & MonthMask;
        var year = BaseYear + (word >> YearShift & YearMask) + (word >> AgeShift & AgeMask) * YearsPerAge;
        try { return new(year, month, day, 0, 0, 0, TimeSpan.Zero); }
        catch (ArgumentOutOfRangeException) { return null; }
    }
}
