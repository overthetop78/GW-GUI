using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterLedMatrix
{
    internal static void Apply(float[] colors, int width, int height,
        EmulationLedMatrixVideoConfiguration configuration)
    {
        var source = colors.ToArray();
        var period = 2 + (int)MathF.Round(configuration.CellSize / 100f * 6f);
        var activeRadius = 0.5f - configuration.CellGap / 100f * 0.36f;
        var diffusion = configuration.Diffusion / 100f * 0.55f;
        var brightness = 0.35f + configuration.Brightness / 100f * 1.25f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var average = CellAverage(source, width, height, x / period, y / period, period);
            var emission = Color(average, configuration.Color);
            var localX = ((x % period) + 0.5f) / period - 0.5f;
            var localY = ((y % period) + 0.5f) / period - 0.5f;
            var distance = MathF.Sqrt(localX * localX + localY * localY);
            var cellMask = Math.Clamp((activeRadius - distance) * period, 0f, 1f);
            var haloMask = Math.Clamp(1f - distance / 0.71f, 0f, 1f) * diffusion;
            var mask = Math.Clamp(cellMask + haloMask * (1f - cellMask), 0f, 1f);
            var index = (y * width + x) * 3;
            colors[index] = Math.Clamp(emission.R * brightness * mask, 0f, 1f);
            colors[index + 1] = Math.Clamp(emission.G * brightness * mask, 0f, 1f);
            colors[index + 2] = Math.Clamp(emission.B * brightness * mask, 0f, 1f);
        }
    }

    private static (float R, float G, float B) CellAverage(float[] source, int width, int height,
        int cellX, int cellY, int period)
    {
        var startX = cellX * period;
        var startY = cellY * period;
        var endX = Math.Min(startX + period, width);
        var endY = Math.Min(startY + period, height);
        var red = 0f;
        var green = 0f;
        var blue = 0f;
        var count = 0;
        for (var y = startY; y < endY; y++)
        for (var x = startX; x < endX; x++)
        {
            var index = (y * width + x) * 3;
            red += source[index];
            green += source[index + 1];
            blue += source[index + 2];
            count++;
        }
        return (red / count, green / count, blue / count);
    }

    private static (float R, float G, float B) Color((float R, float G, float B) source,
        EmulationLedMatrixColor color)
    {
        if (color == EmulationLedMatrixColor.Rgb) return source;
        var luminance = source.R * 0.2126f + source.G * 0.7152f + source.B * 0.0722f;
        var tint = color switch
        {
            EmulationLedMatrixColor.Red => (1f, 0.03f, 0.01f),
            EmulationLedMatrixColor.Green => (0.03f, 1f, 0.08f),
            EmulationLedMatrixColor.Amber => (1f, 0.42f, 0.02f),
            EmulationLedMatrixColor.Blue => (0.03f, 0.25f, 1f),
            _ => (1f, 1f, 1f)
        };
        return (luminance * tint.Item1, luminance * tint.Item2, luminance * tint.Item3);
    }
}
