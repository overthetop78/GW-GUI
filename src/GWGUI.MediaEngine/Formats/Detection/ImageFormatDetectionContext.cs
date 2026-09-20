using System.IO;
using GWGUI.MediaEngine.Formats;
namespace GWGUI.MediaEngine.Formats.Detection;

internal sealed record ImageFormatDetectionContext(
    IImageFormatCatalog Catalog,
    string FilePath,
    long? KnownLength,
    string Extension,
    IReadOnlyList<DiskFormat> Candidates,
    Func<string, Stream> OpenRead)
{
    public DetectedImageFormat Result(string? formatId, FormatConfidence confidence, string explanationKey) =>
        new(
            Extension,
            Catalog.Formats.FirstOrDefault(format => format.Id.Equals(formatId, StringComparison.OrdinalIgnoreCase)),
            formatId is null ? FormatConfidence.Ambiguous : confidence,
            Candidates,
            explanationKey);

    public DetectedImageFormat Ambiguous(string explanationKey) =>
        new(Extension, null, FormatConfidence.Ambiguous, Candidates, explanationKey);
}
