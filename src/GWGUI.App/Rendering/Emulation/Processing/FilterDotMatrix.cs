using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrix
{
    private const int DotPitch = 4;

    internal static void Apply(float[] colors, int width, int height,
        EmulationDotMatrixVideoConfiguration configuration)
    {
        var source = colors.ToArray();
        var (background, foreground) = Palette(configuration.Palette);
        var radius = 0.14f + configuration.DotSize / 100f * 0.34f;
        var contrast = 0.5f + configuration.Contrast / 100f * 2f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var luminance = CellLuminance(source, width, height,
                x / DotPitch, y / DotPitch);
            var activation = Math.Clamp((luminance - 0.5f) * contrast + 0.5f, 0f, 1f);
            var localX = MathF.Abs(((x % DotPitch) + 0.5f) / DotPitch - 0.5f);
            var localY = MathF.Abs(((y % DotPitch) + 0.5f) / DotPitch - 0.5f);
            var distance = configuration.Shape == EmulationDotMatrixShape.Square
                ? MathF.Max(localX, localY)
                : MathF.Sqrt(localX * localX + localY * localY);
            var mask = Math.Clamp((radius - distance) * DotPitch * 2f, 0f, 1f);
            var level = activation * mask;
            var index = (y * width + x) * 3;
            colors[index] = Lerp(background.R, foreground.R, level);
            colors[index + 1] = Lerp(background.G, foreground.G, level);
            colors[index + 2] = Lerp(background.B, foreground.B, level);
        }
    }

    private static float CellLuminance(float[] source, int width, int height, int cellX, int cellY)
    {
        var total = 0f;
        var count = 0;
        for (var y = cellY * DotPitch; y < Math.Min((cellY + 1) * DotPitch, height); y++)
        for (var x = cellX * DotPitch; x < Math.Min((cellX + 1) * DotPitch, width); x++)
        {
            var index = (y * width + x) * 3;
            total += source[index] * 0.2126f + source[index + 1] * 0.7152f
                + source[index + 2] * 0.0722f;
            count++;
        }
        return total / count;
    }

    private static ((float R, float G, float B) Background,
        (float R, float G, float B) Foreground) Palette(EmulationDotMatrixPalette palette) =>
        palette switch
        {
            EmulationDotMatrixPalette.Gray => ((0.62f, 0.65f, 0.61f), (0.04f, 0.05f, 0.04f)),
            EmulationDotMatrixPalette.Amber => ((0.08f, 0.02f, 0.005f), (1f, 0.42f, 0.015f)),
            EmulationDotMatrixPalette.Blue => ((0.01f, 0.025f, 0.14f), (0.38f, 0.72f, 1f)),
            _ => ((0.42f, 0.56f, 0.24f), (0.025f, 0.055f, 0.015f))
        };

    private static float Lerp(float start, float end, float amount) =>
        start + (end - start) * amount;
}
