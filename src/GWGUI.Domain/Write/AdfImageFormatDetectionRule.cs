namespace GWGUI.Domain.Write;

internal sealed class AdfImageFormatDetectionRule : IImageFormatDetectionRule
{
    public bool TryDetect(ImageFormatDetectionContext context, out DetectedImageFormat result)
    {
        if (context.Extension != ".adf")
        {
            result = null!;
            return false;
        }

        result = context.KnownLength switch
        {
            901120 => context.Result("amiga.amigados", FormatConfidence.Certain, "Detection.AmigaSize"),
            1802240 => context.Result("amiga.amigados_hd", FormatConfidence.Certain, "Detection.AmigaSize"),
            819200 or 820224 => context.Result("acorn.adfs.800", FormatConfidence.Certain, "Detection.AcornSize"),
            _ => context.Ambiguous("Detection.Multiple")
        };
        return context.KnownLength is 901120 or 1802240 or 819200 or 820224;
    }
}
