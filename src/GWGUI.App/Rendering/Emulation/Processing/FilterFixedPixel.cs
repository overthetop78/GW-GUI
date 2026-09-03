using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterFixedPixel
{
    private const float RedLuminance = 0.2126f;
    private const float GreenLuminance = 0.7152f;
    private const float BlueLuminance = 0.0722f;

    internal static void Apply(float[] colors, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, EmulationFixedPixelVideoConfiguration configuration)
    {
        ApplySubpixels(colors, sourceWidth, outputWidth, outputHeight,
            configuration.Subpixels, configuration.MonochromeColorArgb,
            configuration.GridIntensity);
        ApplyGrid(colors, sourceWidth, sourceHeight, outputWidth, outputHeight,
            configuration.GridIntensity, configuration.PixelGap);
        ApplyTechnology(colors, configuration);
    }

    internal static void ApplySubpixels(float[] colors, int sourceWidth,
        int outputWidth, int outputHeight, EmulationSubpixelLayout layout,
        uint? monochromeColorArgb, int gridIntensity)
    {
        if (layout == EmulationSubpixelLayout.Monochrome)
        {
            var tint = LinearTint(monochromeColorArgb ?? 0xFF8FAA6Au);
            for (var index = 0; index < colors.Length; index += 3)
            {
                var luminance = colors[index] * RedLuminance
                    + colors[index + 1] * GreenLuminance
                    + colors[index + 2] * BlueLuminance;
                colors[index] = luminance * tint.Red;
                colors[index + 1] = luminance * tint.Green;
                colors[index + 2] = luminance * tint.Blue;
            }
            return;
        }

        if (gridIntensity == 0) return;
        var attenuation = gridIntensity / 100f * 0.35f;
        var scaleX = sourceWidth / (float)outputWidth;
        for (var y = 0; y < outputHeight; y++)
        for (var x = 0; x < outputWidth; x++)
        {
            var sourceX = (x + 0.5f) * scaleX;
            var fraction = sourceX - MathF.Floor(sourceX);
            var selected = Math.Min(2, (int)(fraction * 3f));
            if (layout == EmulationSubpixelLayout.Bgr) selected = 2 - selected;
            var index = (y * outputWidth + x) * 3;
            for (var channel = 0; channel < 3; channel++)
                if (channel != selected) colors[index + channel] *= 1f - attenuation;
        }
    }

    internal static void ApplyGrid(float[] colors, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, int intensitySetting, int gapSetting)
    {
        if (intensitySetting == 0 || gapSetting == 0) return;
        var intensity = intensitySetting / 100f;
        var halfGap = gapSetting / 100f * 0.45f;
        var scaleX = sourceWidth / (float)outputWidth;
        var scaleY = sourceHeight / (float)outputHeight;
        for (var y = 0; y < outputHeight; y++)
        {
            var sourceY = (y + 0.5f) * scaleY;
            var fractionY = sourceY - MathF.Floor(sourceY);
            var distanceY = Math.Min(fractionY, 1f - fractionY);
            for (var x = 0; x < outputWidth; x++)
            {
                var sourceX = (x + 0.5f) * scaleX;
                var fractionX = sourceX - MathF.Floor(sourceX);
                var distanceX = Math.Min(fractionX, 1f - fractionX);
                var edge = Math.Min(distanceX, distanceY);
                if (edge >= halfGap) continue;
                var coverage = 1f - edge / Math.Max(halfGap, float.Epsilon);
                var factor = 1f - intensity * coverage;
                var index = (y * outputWidth + x) * 3;
                colors[index] *= factor;
                colors[index + 1] *= factor;
                colors[index + 2] *= factor;
            }
        }
    }

    internal static void ApplyTechnology(float[] colors,
        EmulationFixedPixelVideoConfiguration configuration)
    {
        if (configuration.Technology != EmulationFixedPixelTechnology.Oled
            && configuration.BacklightIntensity is int backlight)
        {
            var gain = 0.5f + backlight / 100f * 0.5f;
            for (var index = 0; index < colors.Length; index++) colors[index] *= gain;
        }

        if (configuration.BlackDepth is not int blackDepth) return;
        var floor = (1f - blackDepth / 100f) * 0.12f;
        for (var index = 0; index < colors.Length; index++)
            colors[index] = floor + colors[index] * (1f - floor);
    }

    private static (float Red, float Green, float Blue) LinearTint(uint argb) => (
        SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(((argb >> 16) & 0xff) / 255f),
        SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(((argb >> 8) & 0xff) / 255f),
        SoftwareEmulationVideoProcessingPipeline.SrgbToLinear((argb & 0xff) / 255f));
}
