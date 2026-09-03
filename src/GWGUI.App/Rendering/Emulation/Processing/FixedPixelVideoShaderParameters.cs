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
        var tint = LinearTint(fixedPixel.MonochromeColorArgb ?? 0xFF8FAA6Au);
        return new FixedPixelVideoShaderParameters(
            new Vector4(
                configuration.DisplayTechnology == EmulationVideoDisplayTechnology.FixedPixel
                    ? 1f : 0f,
                (float)fixedPixel.Technology,
                (float)fixedPixel.Subpixels,
                fixedPixel.GridIntensity / 100f),
            new Vector4(fixedPixel.PixelGap / 100f, tint.X, tint.Y, tint.Z),
            new Vector4(OptionalRatio(fixedPixel.BacklightIntensity),
                OptionalRatio(fixedPixel.BlackDepth), 0f, 0f),
            new Vector4(fixedPixel.ResponseTimeMilliseconds,
                fixedPixel.PersistenceIntensity / 100f,
                hasHistory ? 1f : 0f,
                (float)Math.Max(0, elapsedMilliseconds)));
    }

    private static Vector3 LinearTint(uint argb) => new(
        SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(((argb >> 16) & 0xff) / 255f),
        SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(((argb >> 8) & 0xff) / 255f),
        SoftwareEmulationVideoProcessingPipeline.SrgbToLinear((argb & 0xff) / 255f));

    private static float OptionalRatio(int? value) => value is null ? -1f : value.Value / 100f;
}
