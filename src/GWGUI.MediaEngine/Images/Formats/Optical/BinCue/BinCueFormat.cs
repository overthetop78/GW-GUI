using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Images.Formats.Optical.BinCue;

/// <summary>Declares the descriptor and associated files supported by the initial BIN/CUE profile.</summary>
internal static class BinCueFormat
{
    public static readonly IReadOnlySet<string> Extensions = new[]
    {
        DiskImageFileExtensions.Cue
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlySet<string> AssociatedFileExtensions = new[]
    {
        DiskImageFileExtensions.Bin
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static string FormatId => OpticalImageFormatIds.BinCue;
    public static bool CanRead => true;
    public static bool CanWrite => true;
}
