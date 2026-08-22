using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
namespace GWGUI.Domain.Conversion;

public static class ConversionSourceCompatibility
{
    public static IReadOnlyList<DiskFormat> GetOutputs(IImageFormatCatalog catalog, string? sourceExtension, DetectedImageFormat? detection = null)
    {
        if (string.IsNullOrWhiteSpace(sourceExtension)) return catalog.Formats;
        var extension = sourceExtension.StartsWith('.') ? sourceExtension.ToLowerInvariant() : "." + sourceExtension.ToLowerInvariant();
        if (extension is ".scp" or ".hfe" || detection?.Format is null || detection.RequiresUserChoice)
            return catalog.GetCompatibleOutputs(extension);
        return catalog.Formats.Where(format => format.Id == detection.Format.Id || format.Id == "raw.scp").ToArray();
    }
}
