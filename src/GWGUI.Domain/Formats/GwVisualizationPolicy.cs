using GWGUI.Domain.Write;

namespace GWGUI.Domain.Formats;

public static class GwVisualizationPolicy
{
    public static bool CanConvertToScp(string sourcePath, DetectedImageFormat detection, GwFormatCapabilities capabilities)
    {
        if (detection.Format is null || !capabilities.IsKnown) return false;

        var extension = Path.GetExtension(sourcePath);
        return !extension.Equals(".scp", StringComparison.OrdinalIgnoreCase)
            && capabilities.ImageExtensions.Contains(extension)
            && capabilities.ImageExtensions.Contains(".scp");
    }
}
