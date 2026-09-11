using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Formats.Tape.SpectrumTap;

/// <summary>Declares the headerless Spectrum TAP block stream, which requires structural validation or explicit context.</summary>
internal static class SpectrumTapFormat
{
    public static string FormatId => TapeImageFormatIds.SpectrumTap;
    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Tap }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static bool CanRead => true;
    public static bool CanWrite => true;
}
