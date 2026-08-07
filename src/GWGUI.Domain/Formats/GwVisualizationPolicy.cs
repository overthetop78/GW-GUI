using GWGUI.Domain.Write;

namespace GWGUI.Domain.Formats;

public static class GwVisualizationPolicy
{
    public static bool CanConvertToScp(string sourcePath, DetectedImageFormat detection, GwFormatCapabilities capabilities)
    {
        if (detection.Format is null || !capabilities.IsKnown) return false;

        var extension = Path.GetExtension(sourcePath);
        if (extension.Equals(".atr", StringComparison.OrdinalIgnoreCase))
            return detection.Format.Id is "atari.90" or "atari.130"
                && capabilities.FormatIds.Contains(detection.Format.Id)
                && capabilities.ImageExtensions.Contains(".img")
                && capabilities.ImageExtensions.Contains(".scp");

        var gwFormat = GwFormatArgument.FromCatalogId(detection.Format.Id);
        return !extension.Equals(".scp", StringComparison.OrdinalIgnoreCase)
            && gwFormat is not null
            && capabilities.FormatIds.Contains(gwFormat)
            && capabilities.ImageExtensions.Contains(extension)
            && capabilities.ImageExtensions.Contains(".scp");
    }
}
