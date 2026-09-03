namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterGrain
{
    public static void Apply(float[] colors, int width, int height, long sequence, int intensity)
    {
        if (intensity <= 0) return;
        var amount = intensity / 100f * 0.07f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var hash = unchecked((uint)(x * 1597334677) ^ (uint)(y * 3812015801L)
                ^ (uint)(sequence * 95868931L));
            hash ^= hash >> 16;
            hash *= 2246822519u;
            var noise = ((hash & 2047) / 1023.5f - 1f) * amount;
            var offset = (y * width + x) * 3;
            for (var channel = 0; channel < 3; channel++)
                colors[offset + channel] = Math.Clamp(colors[offset + channel] + noise, 0f, 1f);
        }
    }
}
