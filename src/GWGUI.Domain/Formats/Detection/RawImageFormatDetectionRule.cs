namespace GWGUI.Domain.Formats.Detection;

internal sealed class RawImageFormatDetectionRule : IImageFormatDetectionRule
{
    public bool TryDetect(ImageFormatDetectionContext context, out DetectedImageFormat result)
    {
        if (context.Extension != ".scp")
        {
            result = null!;
            return false;
        }

        result = context.Result("raw.scp", FormatConfidence.Certain, "Detection.RawScp");
        return true;
    }
}
