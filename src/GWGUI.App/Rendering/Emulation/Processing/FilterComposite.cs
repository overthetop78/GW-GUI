namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterComposite
{
    public static void Apply(float[] colors, int width, int height, long sequence, int intensity)
    {
        if (intensity <= 0 || width < 2) return;
        var source = colors.ToArray();
        var amount = intensity / 100f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var luminance = 0f;
            var inPhase = 0f;
            var quadrature = 0f;
            var lumaWeight = 0f;
            var chromaSamples = 0;
            for (var offset = -2; offset <= 2; offset++)
            {
                var sampleX = Math.Clamp(x + offset, 0, width - 1);
                var index = (y * width + sampleX) * 3;
                var red = source[index];
                var green = source[index + 1];
                var blue = source[index + 2];
                if (Math.Abs(offset) <= 1)
                {
                    var weight = offset == 0 ? 2f : 1f;
                    luminance += (red * 0.299f + green * 0.587f + blue * 0.114f) * weight;
                    lumaWeight += weight;
                }
                inPhase += red * 0.596f - green * 0.274f - blue * 0.322f;
                quadrature += red * 0.211f - green * 0.523f + blue * 0.312f;
                chromaSamples++;
            }
            luminance /= lumaWeight;
            inPhase /= chromaSamples;
            quadrature /= chromaSamples;
            var center = (y * width + x) * 3;
            var originalY = source[center] * 0.299f + source[center + 1] * 0.587f
                + source[center + 2] * 0.114f;
            var originalI = source[center] * 0.596f - source[center + 1] * 0.274f
                - source[center + 2] * 0.322f;
            var originalQ = source[center] * 0.211f - source[center + 1] * 0.523f
                + source[center + 2] * 0.312f;
            var crawl = (((x + y + sequence) & 1) == 0 ? 1f : -1f) * amount * 0.025f;
            var yValue = Lerp(originalY, luminance, amount * 0.45f);
            var iValue = Lerp(originalI, inPhase, amount) + crawl;
            var qValue = Lerp(originalQ, quadrature, amount) - crawl;
            colors[center] = Math.Clamp(yValue + 0.956f * iValue + 0.621f * qValue, 0f, 1f);
            colors[center + 1] = Math.Clamp(yValue - 0.272f * iValue - 0.647f * qValue, 0f, 1f);
            colors[center + 2] = Math.Clamp(yValue - 1.106f * iValue + 1.703f * qValue, 0f, 1f);
        }
    }

    private static float Lerp(float from, float to, float amount) => from + (to - from) * amount;
}
