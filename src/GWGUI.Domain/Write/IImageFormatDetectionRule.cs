namespace GWGUI.Domain.Write;

internal interface IImageFormatDetectionRule
{
    bool TryDetect(ImageFormatDetectionContext context, out DetectedImageFormat result);
}
