using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Formats.Tape.Uef;

/// <summary>Declares UEF 0.10 tape chunks in direct or explicitly gzip-compressed containers.</summary>
internal static class UefFormat
{
    public static string FormatId => TapeImageFormatIds.Uef;
    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Uef }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static bool CanRead => true;
    public static bool CanWrite => true;
    public static bool SupportsGzipContainer => true;
}
