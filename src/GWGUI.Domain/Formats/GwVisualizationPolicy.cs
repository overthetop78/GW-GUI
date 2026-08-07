using GWGUI.Domain.Write;

namespace GWGUI.Domain.Formats;

public static class GwVisualizationPolicy
{
    public static bool CanConvertToScp(string sourcePath, DetectedImageFormat detection, GwFormatCapabilities capabilities)
    {
        var extension = Path.GetExtension(sourcePath);
        if (!capabilities.IsKnown) return false;
        if (extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase) || extension.Equals(".edsk", StringComparison.OrdinalIgnoreCase))
            return capabilities.ImageExtensions.Contains(extension.ToLowerInvariant()) && capabilities.ImageExtensions.Contains(".scp");
        if (detection.Format is null) return false;
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
