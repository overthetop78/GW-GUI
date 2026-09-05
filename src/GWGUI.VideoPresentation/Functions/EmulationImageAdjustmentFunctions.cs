using GWGUI.VideoPresentation.Constants;
using GWGUI.VideoPresentation.Contracts;

namespace GWGUI.VideoPresentation.Functions;

public static class EmulationImageAdjustmentFunctions
{
    public static EmulationImageAdjustments Normalize(EmulationImageAdjustments? adjustments)
    {
        adjustments ??= new EmulationImageAdjustments();
        return new EmulationImageAdjustments(
            Clamp(adjustments.Brightness),
            Clamp(adjustments.Contrast),
            Clamp(adjustments.Gamma),
            Clamp(adjustments.Saturation),
            Clamp(adjustments.Sharpness));
    }

    public static double GammaExponent(int gamma) => Math.Pow(2d,
        -Clamp(gamma) / EmulationVideoProcessingLimits.GammaConversionScale);

    private static int Clamp(int value) => Math.Clamp(value,
        EmulationVideoProcessingLimits.AdjustmentMinimum,
        EmulationVideoProcessingLimits.AdjustmentMaximum);
}
