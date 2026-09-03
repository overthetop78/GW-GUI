namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterBloom
{
    private const int Radius = 2;
    private const float Threshold = 0.6f;

    public static void Apply(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || width < 1 || height < 1) return;
        var source = colors.ToArray();
        var amount = intensity / 100f * 0.35f;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var output = (y * width + x) * 3;
                for (var channel = 0; channel < 3; channel++)
                {
                    var light = 0f;
                    var samples = 0;
                    for (var sampleY = Math.Max(0, y - Radius);
                         sampleY <= Math.Min(height - 1, y + Radius); sampleY++)
                    {
                        for (var sampleX = Math.Max(0, x - Radius);
                             sampleX <= Math.Min(width - 1, x + Radius); sampleX++)
                        {
                            var value = source[(sampleY * width + sampleX) * 3 + channel];
                            light += Math.Max(0f, value - Threshold) / (1f - Threshold);
                            samples++;
                        }
                    }
                    colors[output + channel] = Math.Clamp(source[output + channel]
                        + light / samples * amount, 0f, 1f);
                }
            }
        }
    }
}
