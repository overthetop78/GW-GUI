using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVector
{
    private const float RedLuminance = 0.2126f;
    private const float GreenLuminance = 0.7152f;
    private const float BlueLuminance = 0.0722f;

    internal static void Apply(float[] colors, int width, int height,
        EmulationVectorVideoConfiguration configuration)
    {
        if (configuration.LineIntensity == 0) return;
        var luminance = new float[width * height];
        for (var pixel = 0; pixel < luminance.Length; pixel++)
        {
            var index = pixel * 3;
            luminance[pixel] = colors[index] * RedLuminance
                + colors[index + 1] * GreenLuminance
                + colors[index + 2] * BlueLuminance;
        }
        var emission = new float[width * height];
        var threshold = configuration.LineThreshold / 100f;
        var intensity = configuration.LineIntensity / 100f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var gradientX = -Sample(luminance, width, height, x - 1, y - 1)
                - 2f * Sample(luminance, width, height, x - 1, y)
                - Sample(luminance, width, height, x - 1, y + 1)
                + Sample(luminance, width, height, x + 1, y - 1)
                + 2f * Sample(luminance, width, height, x + 1, y)
                + Sample(luminance, width, height, x + 1, y + 1);
            var gradientY = -Sample(luminance, width, height, x - 1, y - 1)
                - 2f * Sample(luminance, width, height, x, y - 1)
                - Sample(luminance, width, height, x + 1, y - 1)
                + Sample(luminance, width, height, x - 1, y + 1)
                + 2f * Sample(luminance, width, height, x, y + 1)
                + Sample(luminance, width, height, x + 1, y + 1);
            var magnitude = Math.Clamp(MathF.Sqrt(
                gradientX * gradientX + gradientY * gradientY) / 4f, 0f, 1f);
            var line = SmoothStep(threshold, Math.Min(1f, threshold + 0.1f), magnitude)
                * intensity;
            emission[y * width + x] = line;
            var index = (y * width + x) * 3;
            colors[index] += (1f - colors[index]) * line;
            colors[index + 1] += (1f - colors[index + 1]) * line;
            colors[index + 2] += (1f - colors[index + 2]) * line;
        }

        if (configuration.HaloIntensity == 0) return;
        var halo = configuration.HaloIntensity / 100f * 0.5f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var average = 0f;
            for (var offsetY = -1; offsetY <= 1; offsetY++)
            for (var offsetX = -1; offsetX <= 1; offsetX++)
                average += Sample(emission, width, height, x + offsetX, y + offsetY);
            var light = average / 9f * halo;
            var index = (y * width + x) * 3;
            colors[index] = Math.Clamp(colors[index] + light, 0f, 1f);
            colors[index + 1] = Math.Clamp(colors[index + 1] + light, 0f, 1f);
            colors[index + 2] = Math.Clamp(colors[index + 2] + light, 0f, 1f);
        }
    }

    private static float Sample(float[] values, int width, int height, int x, int y) =>
        values[Math.Clamp(y, 0, height - 1) * width + Math.Clamp(x, 0, width - 1)];

    private static float SmoothStep(float start, float end, float value)
    {
        if (end <= start) return value >= end ? 1f : 0f;
        var amount = Math.Clamp((value - start) / (end - start), 0f, 1f);
        return amount * amount * (3f - 2f * amount);
    }
}
