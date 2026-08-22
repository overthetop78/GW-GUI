namespace GWGUI.Domain.Formats.Detection;

internal sealed class AppleImageFormatDetectionRule : IImageFormatDetectionRule
{
    public bool TryDetect(ImageFormatDetectionContext context, out DetectedImageFormat result)
    {
        result = context.Extension switch
        {
            ".d13" => context.Result("apple2.appledos.113", FormatConfidence.Certain, "Detection.AppleDosOrder"),
            ".do" => context.Result("apple2.appledos.140", FormatConfidence.Certain, "Detection.AppleDosOrder"),
            ".po" => context.Result("apple2.prodos.140", FormatConfidence.Certain, "Detection.AppleProDosOrder"),
            ".2mg" => context.Ambiguous("Detection.AppleContainer"),
            _ => null!
        };
        return context.Extension is ".d13" or ".do" or ".po" or ".2mg";
    }
}
