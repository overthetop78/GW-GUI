namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterFixedPixelLight
{
    internal static void ApplyBacklight(float[] colors, int width, int height,
        float intensity, float blackFloor, float bleed, float gainMinimum, float gainRange)
    {
        var original = bleed > 0f ? colors.ToArray() : colors;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var index = (y * width + x) * 3;
            var localLight = bleed > 0f ? BrightNeighbourhood(original, width, height, x, y) : 0f;
            for (var channel = 0; channel < 3; channel++)
            {
                var lit = colors[index + channel] * (gainMinimum + MathF.Pow(intensity, 0.8f) * gainRange);
                var highlight = Math.Clamp((localLight - 0.45f) / 0.55f, 0f, 1f);
                var halo = MathF.Max(0f, localLight - colors[index + channel]) * bleed * highlight;
                colors[index + channel] = Math.Clamp(blackFloor + (lit + halo) * (1f - blackFloor), 0f, 1f);
            }
        }
    }

    internal static void ApplyEmissive(float[] colors, float blackFloor, float peakGain)
    {
        for (var index = 0; index < colors.Length; index++)
        {
            var value = colors[index];
            var contrast = value * value * (3f - 2f * value);
            colors[index] = Math.Clamp(blackFloor + contrast * peakGain * (1f - blackFloor), 0f, 1f);
        }
    }

    private static float BrightNeighbourhood(float[] colors, int width, int height, int x, int y)
    {
        var left = PixelBrightness(colors, width, Math.Max(0, x - 1), y);
        var right = PixelBrightness(colors, width, Math.Min(width - 1, x + 1), y);
        var up = PixelBrightness(colors, width, x, Math.Max(0, y - 1));
        var down = PixelBrightness(colors, width, x, Math.Min(height - 1, y + 1));
        return (left + right + up + down) * 0.25f;
    }

    private static float PixelBrightness(float[] colors, int width, int x, int y)
    {
        var index = (y * width + x) * 3;
        return MathF.Max(colors[index], MathF.Max(colors[index + 1], colors[index + 2]));
    }
}
