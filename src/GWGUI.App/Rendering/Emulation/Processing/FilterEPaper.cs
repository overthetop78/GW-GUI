using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterEPaper
{
    private static readonly int[,] Bayer4 =
    {
        { 0, 8, 2, 10 }, { 12, 4, 14, 6 },
        { 3, 11, 1, 9 }, { 15, 7, 13, 5 }
    };

    internal static void Apply(float[] colors, int width, int height,
        EmulationEPaperVideoConfiguration configuration)
    {
        var contrast = 0.6f + configuration.Contrast / 100f * 1.8f;
        var dither = configuration.Dithering / 100f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var index = (y * width + x) * 3;
            var threshold = (Bayer4[y & 3, x & 3] + 0.5f) / 16f - 0.5f;
            var red = Contrast(colors[index], contrast);
            var green = Contrast(colors[index + 1], contrast);
            var blue = Contrast(colors[index + 2], contrast);
            if (configuration.ColorMode == EmulationEPaperColorMode.Monochrome)
            {
                var luminance = red * 0.2126f + green * 0.7152f + blue * 0.0722f;
                var level = luminance + threshold * dither * 0.8f >= 0.5f ? 1f : 0f;
                colors[index] = Lerp(0.025f, 0.9f, level);
                colors[index + 1] = Lerp(0.025f, 0.86f, level);
                colors[index + 2] = Lerp(0.02f, 0.74f, level);
                continue;
            }
            if (configuration.ColorMode == EmulationEPaperColorMode.Grayscale16)
            {
                var luminance = red * 0.2126f + green * 0.7152f + blue * 0.0722f;
                var level = Quantize(luminance + threshold * dither / 15f);
                colors[index] = Lerp(0.025f, 0.9f, level);
                colors[index + 1] = Lerp(0.025f, 0.86f, level);
                colors[index + 2] = Lerp(0.02f, 0.74f, level);
                continue;
            }
            colors[index] = 0.04f + Quantize(red + threshold * dither / 15f) * 0.82f;
            colors[index + 1] = 0.04f + Quantize(green + threshold * dither / 15f) * 0.78f;
            colors[index + 2] = 0.035f + Quantize(blue + threshold * dither / 15f) * 0.7f;
        }
    }

    private static float Contrast(float value, float contrast) =>
        Math.Clamp((value - 0.5f) * contrast + 0.5f, 0f, 1f);

    private static float Quantize(float value) => MathF.Round(Math.Clamp(value, 0f, 1f) * 15f) / 15f;

    private static float Lerp(float start, float end, float amount) =>
        start + (end - start) * amount;
}
