namespace GWGUI.Domain.Formats.Detection;

internal interface IImageFormatDetectionRule
{
    bool TryDetect(ImageFormatDetectionContext context, out DetectedImageFormat result);
}
