using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Images.Formats.Tape.MsxCas;

/// <summary>Declares the headerless-timing MSX CAS byte stream identified by repeated binary separators.</summary>
internal static class MsxCasFormat
{
    public static string FormatId => TapeImageFormatIds.MsxCas;
    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Cas }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static bool CanRead => true;
    public static bool CanWrite => true;
    public static bool PreservesTiming => false;
}
