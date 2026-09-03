namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterGrain
{
    internal const string Shader = """
        vec3 filterGrain(vec3 color,float amount,float noise)
        {
            float luminance=dot(color,vec3(.2126,.7152,.0722));
            float visibility=.22+.78*(1.0-abs(luminance*2.0-1.0));
            return clamp(color+vec3(noise*amount*.045*visibility),0.0,1.0);
        }
        """;

    public static void Apply(float[] colors, int width, int height, long sequence, int intensity)
    {
        if (intensity <= 0) return;
        var amount = intensity / 100f * 0.045f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var hash = unchecked((uint)(x * 1597334677) ^ (uint)(y * 3812015801L)
                ^ (uint)(sequence * 95868931L));
            hash ^= hash >> 16;
            hash *= 2246822519u;
            var offset = (y * width + x) * 3;
            var luminance = colors[offset] * 0.2126f + colors[offset + 1] * 0.7152f
                + colors[offset + 2] * 0.0722f;
            var visibility = 0.22f + 0.78f * (1f - MathF.Abs(luminance * 2f - 1f));
            var noise = ((hash & 2047) / 1023.5f - 1f) * amount * visibility;
            for (var channel = 0; channel < 3; channel++)
                colors[offset + channel] = Math.Clamp(colors[offset + channel] + noise, 0f, 1f);
        }
    }
}
