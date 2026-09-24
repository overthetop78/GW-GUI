namespace GWGUI.MediaEngine.Images.Formats.Detection;

internal interface IImageFormatDetectionRule
{
    bool TryDetect(ImageFormatDetectionContext context, out DetectedImageFormat result);
}
