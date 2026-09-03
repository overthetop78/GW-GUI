using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterProjection
{
    internal static void Apply(float[] colors, int width, int height,
        EmulationProjectionVideoConfiguration configuration)
    {
        var source = colors.ToArray();
        var radius = configuration.OpticalBlur > 60 ? 2 : 1;
        var blur = configuration.OpticalBlur / 100f;
        var diffusion = configuration.Diffusion / 100f * 0.35f;
        var texture = configuration.ScreenTexture / 100f * 0.22f;
        var shift = (int)MathF.Round(configuration.Convergence / 100f * 3f);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = (y * width + x) * 3;
            var average = Neighborhood(source, width, height, x, y, radius);
            var red = Lerp(Sample(source, width, height, x - shift, y, 0), average.R, blur);
            var green = Lerp(source[center + 1], average.G, blur);
            var blue = Lerp(Sample(source, width, height, x + shift, y, 2), average.B, blur);
            red = Math.Clamp(red + average.R * diffusion, 0f, 1f);
            green = Math.Clamp(green + average.G * diffusion, 0f, 1f);
            blue = Math.Clamp(blue + average.B * diffusion, 0f, 1f);
            var weave = ((x & 3) == 0 ? 0.65f : 0f) + ((y & 3) == 0 ? 0.35f : 0f);
            var screen = 1f - texture * weave;
            colors[center] = red * screen;
            colors[center + 1] = green * screen;
            colors[center + 2] = blue * screen;
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

    private static float Sample(float[] source, int width, int height, int x, int y, int channel)
    {
        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);
        return source[(y * width + x) * 3 + channel];
    }

    private static float Lerp(float start, float end, float amount) =>
        start + (end - start) * amount;
}
