namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterRf
{
    public static void Apply(float[] colors, int width, int height, long sequence, int intensity)
    {
        if (intensity <= 0) return;
        var source = colors.ToArray();
        var amount = intensity / 100f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = (y * width + x) * 3;
            var left = (y * width + Math.Max(0, x - 1)) * 3;
            var right = (y * width + Math.Min(width - 1, x + 1)) * 3;
            var hash = unchecked((uint)(x * 374761393 + y * 668265263)
                ^ (uint)(sequence * 2246822519L));
            hash = (hash ^ (hash >> 13)) * 1274126177u;
            var noise = ((hash & 1023) / 511.5f - 1f) * amount * 0.08f;
            for (var channel = 0; channel < 3; channel++)
            {
                var blurred = (source[left + channel] + source[center + channel] * 2f
                    + source[right + channel]) * 0.25f;
                var mixed = source[center + channel]
                    + (blurred - source[center + channel]) * amount * 0.65f;
                colors[center + channel] = Math.Clamp(mixed + noise, 0f, 1f);
            }
        }
    }
}
