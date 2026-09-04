namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterPlasmaLightDiffusion
{
    internal const string Shader = """
        vec3 filterPlasmaLightDiffusion(vec3 color,vec3 nearLight,vec3 farLight,float intensity)
        {
            if(intensity<=0.0)return color;
            vec3 light=max(nearLight*.7+farLight*.3-vec3(.12),vec3(0.0))/vec3(.88);
            return clamp(color+light*intensity*.82,0.0,1.0);
        }
        """;

    internal static void Apply(float[] colors, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, int setting)
    {
        if (setting <= 0 || outputWidth <= 0 || outputHeight <= 0) return;
        var source = colors.ToArray();
        var amount = setting / 100f * 0.82f;
        var radiusX = Math.Max(1, (int)MathF.Round(outputWidth / (float)Math.Max(1, sourceWidth)));
        var radiusY = Math.Max(1, (int)MathF.Round(outputHeight / (float)Math.Max(1, sourceHeight)));
        Parallel.For(0, outputHeight, y =>
        {
            for (var x = 0; x < outputWidth; x++)
            for (var channel = 0; channel < 3; channel++)
            {
                var near = CardinalAverage(source, outputWidth, outputHeight, x, y,
                    radiusX, radiusY, channel);
                var far = CardinalAverage(source, outputWidth, outputHeight, x, y,
                    radiusX * 2, radiusY * 2, channel);
                var light = MathF.Max(0f, near * 0.7f + far * 0.3f - 0.12f) / 0.88f;
                var index = (y * outputWidth + x) * 3 + channel;
                colors[index] = Math.Clamp(source[index] + light * amount, 0f, 1f);
            }
        });
    }

    private static float CardinalAverage(float[] colors, int width, int height,
        int x, int y, int radiusX, int radiusY, int channel) =>
        (Sample(colors, width, height, x - radiusX, y, channel)
         + Sample(colors, width, height, x + radiusX, y, channel)
         + Sample(colors, width, height, x, y - radiusY, channel)
         + Sample(colors, width, height, x, y + radiusY, channel)) * 0.25f;

    private static float Sample(float[] colors, int width, int height,
        int x, int y, int channel)
    {
        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);
        return colors[(y * width + x) * 3 + channel];
    }
}
