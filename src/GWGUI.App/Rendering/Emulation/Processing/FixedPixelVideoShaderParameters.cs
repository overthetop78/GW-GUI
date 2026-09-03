using System.Numerics;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal readonly record struct FixedPixelVideoShaderParameters(
    Vector4 Display,
    Vector4 Spatial,
    Vector4 Technology,
    Vector4 Temporal)
{
    internal static FixedPixelVideoShaderParameters From(
        EmulationVideoProcessingConfiguration configuration,
        bool hasHistory = false,
        double elapsedMilliseconds = 0)
    {
        var fixedPixel = configuration.FixedPixel;
        var tint = FilterFixedPixelSubpixels.Tint(fixedPixel.MonochromePalette);
        return new FixedPixelVideoShaderParameters(
            new Vector4(
                configuration.DisplayTechnology == EmulationVideoDisplayTechnology.FixedPixel
                    ? 1f : 0f,
                (float)fixedPixel.Technology,
                (float)fixedPixel.Subpixels,
                fixedPixel.GridIntensity / 100f),
            new Vector4(fixedPixel.PixelGap / 100f, tint.Red, tint.Green, tint.Blue),
            new Vector4(ResolvedBacklight(fixedPixel),
                ResolvedBlackDepth(fixedPixel), fixedPixel.BacklightBleedIntensity / 100f, 0f),
            new Vector4(fixedPixel.ResponseTimeMilliseconds,
                fixedPixel.PersistenceIntensity / 100f,
                hasHistory ? 1f : 0f,
                (float)Math.Max(0, elapsedMilliseconds)));
    }

    private static float ResolvedBacklight(EmulationFixedPixelVideoConfiguration value) =>
        (value.BacklightIntensity ?? (value.Technology == EmulationFixedPixelTechnology.Lcd ? 65 : 80)) / 100f;

    private static float ResolvedBlackDepth(EmulationFixedPixelVideoConfiguration value) =>
        (value.BlackDepth ?? value.Technology switch
        {
            EmulationFixedPixelTechnology.Lcd => 35,
            EmulationFixedPixelTechnology.LedBacklitLcd => 55,
            _ => 100
        }) / 100f;
}
