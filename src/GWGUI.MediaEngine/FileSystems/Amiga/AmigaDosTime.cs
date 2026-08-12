using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Décode les dates AmigaDOS depuis l'époque du 1er janvier 1978 UTC.</summary>
public static class AmigaDosTime
{
    /// <summary>Offset relatif du nombre de jours.</summary>
    public const int DaysOffset = 0;
    /// <summary>Offset relatif du nombre de minutes.</summary>
    public const int MinutesOffset = DaysOffset + BigEndianInt32.Size;
    /// <summary>Offset relatif du nombre de ticks.</summary>
    public const int TicksOffset = MinutesOffset + BigEndianInt32.Size;

    /// <summary>Lit une date valide ou retourne <see langword="null"/>.</summary>
    public static DateTimeOffset? Read(ReadOnlySpan<byte> block, int offset)
    {
        int days;
        int minutes;
        int ticks;
        try
        {
            days = BigEndianInt32.Read(block, offset + DaysOffset);
            minutes = BigEndianInt32.Read(block, offset + MinutesOffset);
            ticks = BigEndianInt32.Read(block, offset + TicksOffset);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
        if (days < 0 || minutes < 0 || minutes >= AmigaDosLayout.MinutesPerDay || ticks < 0 || ticks >= 60 * AmigaDosLayout.TicksPerSecond) return null;
        try
        {
            return AmigaDosLayout.Epoch.AddDays(days).AddMinutes(minutes).AddMilliseconds(ticks * AmigaDosLayout.TickDurationMilliseconds);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }
}
