using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrix
{
    internal static void Apply(float[] colors, int sourceWidth, int sourceHeight,
        int width, int height,
        EmulationDotMatrixVideoConfiguration configuration)
    {
        var source = colors.ToArray();
        var pitch = FilterDotMatrixCellSize.Pitch(configuration.CellSize);
        var columns = (sourceWidth + pitch - 1) / pitch;
        var rows = (sourceHeight + pitch - 1) / pitch;
        var averages = new float[checked(columns * rows * 3)];
        var counts = new int[checked(columns * rows)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var sourceX = Math.Min(sourceWidth - 1, x * sourceWidth / width);
            var sourceY = Math.Min(sourceHeight - 1, y * sourceHeight / height);
            var cell = sourceY / pitch * columns + sourceX / pitch;
            var pixel = (y * width + x) * 3;
            averages[cell * 3] += source[pixel];
            averages[cell * 3 + 1] += source[pixel + 1];
            averages[cell * 3 + 2] += source[pixel + 2];
            counts[cell]++;
        }
        for (var cell = 0; cell < counts.Length; cell++)
        {
            var count = Math.Max(1, counts[cell]);
            averages[cell * 3] /= count;
            averages[cell * 3 + 1] /= count;
            averages[cell * 3 + 2] /= count;
        }

        var radius = FilterDotMatrixDotSize.Radius(configuration.DotSize,
            configuration.CellGap);
        var edge = MathF.Max(sourceWidth / (float)width, sourceHeight / (float)height) / pitch;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var logicalX = (x + .5f) * sourceWidth / width;
            var logicalY = (y + .5f) * sourceHeight / height;
            var cellX = Math.Min(columns - 1, (int)(logicalX / pitch));
            var cellY = Math.Min(rows - 1, (int)(logicalY / pitch));
            var localX = logicalX / pitch - MathF.Floor(logicalX / pitch) - .5f;
            var localY = logicalY / pitch - MathF.Floor(logicalY / pitch) - .5f;
            var distance = FilterDotMatrixShape.Distance(localX, localY,
                configuration.Shape);
            var core = SmoothStep(radius + edge, radius - edge, distance);
            var halo = FilterDotMatrixHalo.Apply(distance, radius,
                configuration.HaloIntensity) * (1f - core);
            var average = (cellY * columns + cellX) * 3;
            var palette = FilterDotMatrixPalette.Apply(averages[average],
                averages[average + 1], averages[average + 2], configuration.Palette);
            var index = (y * width + x) * 3;
            var luminance = averages[average] * .2126f + averages[average + 1] * .7152f
                + averages[average + 2] * .0722f;
            var activation = FilterDotMatrixBrightness.Apply(
                FilterDotMatrixContrast.Apply(luminance, configuration.Contrast),
                configuration.Brightness);
            var light = activation * (core + halo);
            colors[index] = Math.Clamp(Lerp(palette.Background.R,
                palette.Foreground.R, light), 0f, 1f);
            colors[index + 1] = Math.Clamp(Lerp(palette.Background.G,
                palette.Foreground.G, light), 0f, 1f);
            colors[index + 2] = Math.Clamp(Lerp(palette.Background.B,
                palette.Foreground.B, light), 0f, 1f);
        }
    }

    private static float SmoothStep(float edge0, float edge1, float value)
    {
        var position = Math.Clamp((value - edge0) / (edge1 - edge0), 0f, 1f);
        return position * position * (3f - 2f * position);
    }

    private static float Lerp(float start, float end, float amount) =>
        start + (end - start) * amount;
}
