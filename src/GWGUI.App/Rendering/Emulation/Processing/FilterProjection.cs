using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterProjection
{
    internal static void Apply(float[] colors, int width, int height,
        EmulationProjectionVideoConfiguration configuration)
    {
        var source = colors.ToArray();
        var radius = 2;
        var blur = configuration.OpticalBlur / 100f;
        var diffusion = configuration.Diffusion / 100f;
        var texture = configuration.ScreenTexture / 100f;
        var shift = FilterProjectionConvergence.Apply(configuration.Convergence / 100f);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = (y * width + x) * 3;
            var average = Neighborhood(source, width, height, x, y, radius);
            var red = FilterProjectionOpticalBlur.Apply(Sample(source, width, height, x - shift, y, 0), average.R, blur);
            var green = FilterProjectionOpticalBlur.Apply(source[center + 1], average.G, blur);
            var blue = FilterProjectionOpticalBlur.Apply(Sample(source, width, height, x + shift, y, 2), average.B, blur);
            red = FilterProjectionDiffusion.Apply(red, average.R, diffusion);
            green = FilterProjectionDiffusion.Apply(green, average.G, diffusion);
            blue = FilterProjectionDiffusion.Apply(blue, average.B, diffusion);
            float Compose(float value)
            {
                value = FilterProjectionLightOutput.Apply(value, configuration.LightOutput / 100f);
                value = FilterProjectionVignette.Apply(value, (x + .5f) / width,
                    (y + .5f) / height, configuration.Vignette / 100f);
                value = FilterProjectionAmbientLight.Apply(value, configuration.AmbientLight / 100f);
                return FilterProjectionScreenTexture.Apply(value, x, y, texture);
            }
            colors[center] = Compose(red);
            colors[center + 1] = Compose(green);
            colors[center + 2] = Compose(blue);
        }
    }

    private static (float R, float G, float B) Neighborhood(float[] source, int width, int height,
        int x, int y, int radius)
    {
        var red = 0f;
        var green = 0f;
        var blue = 0f;
        var count = 0;
        for (var offsetY = -radius; offsetY <= radius; offsetY++)
        for (var offsetX = -radius; offsetX <= radius; offsetX++)
        {
            var sampleX = Math.Clamp(x + offsetX, 0, width - 1);
            var sampleY = Math.Clamp(y + offsetY, 0, height - 1);
            var index = (sampleY * width + sampleX) * 3;
            red += source[index];
            green += source[index + 1];
            blue += source[index + 2];
            count++;
        }
        return (red / count, green / count, blue / count);
    }

    private static float Sample(float[] source, int width, int height, float x, int y, int channel)
    {
        x = Math.Clamp(x, 0f, width - 1f);
        y = Math.Clamp(y, 0, height - 1);
        var left = (int)MathF.Floor(x);
        var right = Math.Min(width - 1, left + 1);
        return Lerp(source[(y * width + left) * 3 + channel],
            source[(y * width + right) * 3 + channel], x - left);
    }

    private static float Lerp(float start, float end, float amount) =>
        start + (end - start) * amount;
}
