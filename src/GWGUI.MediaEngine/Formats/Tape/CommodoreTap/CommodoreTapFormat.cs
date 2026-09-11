using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Formats.Tape.CommodoreTap;

/// <summary>Declares Commodore TAP images for C64, VIC-20, C16, PET, and CBM-II platforms.</summary>
internal static class CommodoreTapFormat
{
    public static string FormatId => TapeImageFormatIds.CommodoreTap;
    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Tap }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlySet<string> Signatures =
        new[] { CommodoreTapConstants.C64Signature, CommodoreTapConstants.C16Signature }
            .ToFrozenSet(StringComparer.Ordinal);
    public static readonly IReadOnlySet<byte> Versions =
        new[]
        {
            CommodoreTapConstants.OriginalVersion,
            CommodoreTapConstants.ExactPulseVersion,
            CommodoreTapConstants.HalfWaveVersion
        }.ToFrozenSet();

    public static bool CanRead => true;
    public static bool CanWrite => true;
}
