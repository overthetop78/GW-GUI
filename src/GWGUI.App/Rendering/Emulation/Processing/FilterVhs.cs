namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVhs
{
    public static void Apply(float[] colors, int width, int height, long sequence, int intensity)
    {
        if (intensity <= 0 || width < 2) return;
        var source = colors.ToArray();
        var amount = intensity / 100f;
        for (var y = 0; y < height; y++)
        {
            var hash = unchecked((uint)(y * 1103515245L + sequence * 12345L));
            hash ^= hash >> 15;
            var shift = (int)MathF.Round((((hash & 7) / 3.5f) - 1f) * amount * 3f);
            var dropout = (y + sequence * 3L) % 17L == 0 ? amount * 0.45f : 0f;
            for (var x = 0; x < width; x++)
            {
                var output = (y * width + x) * 3;
                var centerX = Math.Clamp(x + shift, 0, width - 1);
                var redX = Math.Clamp(centerX + 1, 0, width - 1);
                var blueX = Math.Clamp(centerX - 1, 0, width - 1);
                var center = (y * width + centerX) * 3;
                var red = (y * width + redX) * 3;
                var blue = (y * width + blueX) * 3;
                colors[output] = Math.Clamp(Lerp(source[center], source[red], amount * 0.45f)
                    * (1f - dropout), 0f, 1f);
                colors[output + 1] = source[center + 1] * (1f - dropout);
                colors[output + 2] = Math.Clamp(Lerp(source[center + 2], source[blue + 2],
                    amount * 0.45f) * (1f - dropout), 0f, 1f);
            }
        }
    }

    private static float Lerp(float from, float to, float amount) => from + (to - from) * amount;
}
