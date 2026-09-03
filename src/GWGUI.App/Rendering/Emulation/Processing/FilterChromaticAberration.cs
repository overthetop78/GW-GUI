namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterChromaticAberration
{
    public static void Apply(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || width < 2) return;
        var source = colors.ToArray();
        var offset = intensity / 100f * 3f;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var output = (y * width + x) * 3;
                colors[output] = Sample(source, width, y, x + offset, 0);
                colors[output + 1] = source[output + 1];
                colors[output + 2] = Sample(source, width, y, x - offset, 2);
            }
        }
    }

    private static float Sample(float[] colors, int width, int y, float x, int channel)
    {
        var clamped = Math.Clamp(x, 0f, width - 1f);
        var left = (int)clamped;
        var right = Math.Min(left + 1, width - 1);
        var amount = clamped - left;
        var row = y * width;
        var from = colors[(row + left) * 3 + channel];
        var to = colors[(row + right) * 3 + channel];
        return from + (to - from) * amount;
    }
}
