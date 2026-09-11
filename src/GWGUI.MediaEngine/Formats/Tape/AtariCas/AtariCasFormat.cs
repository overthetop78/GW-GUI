using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Formats.Tape.AtariCas;

/// <summary>Declares the chunked Atari CAS profile identified by its FUJI marker.</summary>
internal static class AtariCasFormat
{
    public static string FormatId => TapeImageFormatIds.AtariCas;
    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Cas }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlySet<string> UnderstoodChunks =
        new[]
        {
            AtariCasConstants.FileMarkerChunk,
            AtariCasConstants.BaudRateChunk,
            AtariCasConstants.DataChunk,
            AtariCasConstants.FskChunk
        }.ToFrozenSet(StringComparer.Ordinal);
    public static bool CanRead => true;
    public static bool CanWrite => true;
}
