using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Décode les dates et heures ProDOS.</summary>
internal static class ProDosDateTime
{
    /// <summary>Définit la valeur ProDOS nommée <c>BaseYear</c>.</summary>
    public const int BaseYear = 1900;
    /// <summary>Définit la valeur ProDOS nommée <c>PivotYear</c>.</summary>
    public const int PivotYear = 1940;
    /// <summary>Définit la valeur ProDOS nommée <c>YearShift</c>.</summary>
    public const int YearShift = 9;
    /// <summary>Définit la valeur ProDOS nommée <c>MonthShift</c>.</summary>
    public const int MonthShift = 5;
    /// <summary>Définit la valeur ProDOS nommée <c>MonthMask</c>.</summary>
    public const int MonthMask = 0x0f;
    /// <summary>Définit la valeur ProDOS nommée <c>DayMask</c>.</summary>
    public const int DayMask = 0x1f;
    /// <summary>Définit la valeur ProDOS nommée <c>HourShift</c>.</summary>
    public const int HourShift = 8;
    /// <summary>Définit la valeur ProDOS nommée <c>MinuteMask</c>.</summary>
    public const int MinuteMask = 0x3f;

    /// <summary>Décode une date et une heure little-endian, ou retourne une absence lorsque leurs champs sont impossibles.</summary>
    public static DateTimeOffset? Read(ReadOnlySpan<byte> data, int offset)
    {
        if (offset + sizeof(ushort) * 2 > data.Length) return null;
        var date = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, sizeof(ushort)));
        var time = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset + sizeof(ushort), sizeof(ushort)));
        var year = BaseYear + (date >> YearShift);
        if (year < PivotYear) year += 100;
        try { return new DateTimeOffset(year, (date >> MonthShift) & MonthMask, date & DayMask, time >> HourShift, time & MinuteMask, 0, TimeSpan.Zero); }
        catch (ArgumentOutOfRangeException) { return null; }
    }
}
