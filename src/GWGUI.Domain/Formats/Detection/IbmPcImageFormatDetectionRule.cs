namespace GWGUI.Domain.Formats.Detection;

internal sealed class IbmPcImageFormatDetectionRule : IImageFormatDetectionRule
{
    public bool TryDetect(ImageFormatDetectionContext context, out DetectedImageFormat result)
    {
        if (context.Extension is not (".ima" or ".img"))
        {
            result = null!;
            return false;
        }

        var formatId = context.KnownLength switch
        {
            163840 => "ibm.160", 184320 => "ibm.180", 327680 => "ibm.320", 368640 => "ibm.360",
            737280 => "ibm.720", 819200 => "ibm.800", 1228800 => "ibm.1200", 1474560 => "ibm.1440",
            1720320 => "ibm.1680", 2949120 => "ibm.2880", _ => null
        };
        result = formatId is null
            ? context.Ambiguous("Detection.IbmAmbiguous")
            : context.Result(formatId, FormatConfidence.Certain, "Detection.IbmSize");
        return true;
    }
}
