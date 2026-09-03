namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class SignalConnectionSVideo
{
    internal const string Shader = """
        vec3 signalConnectionSVideo(vec3 color,vec3 left,vec3 right,float amount)
        { return signalConnectionComponent(color,left,right,amount*(.52/.34)); }
        """;

    public static void Apply(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || width < 2) return;
        var source = colors.ToArray();
        var amount = intensity / 100f * 0.52f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = (y * width + x) * 3;
            var red = source[center];
            var green = source[center + 1];
            var blue = source[center + 2];
            var luminance = red * 0.299f + green * 0.587f + blue * 0.114f;
            var originalI = red * 0.596f - green * 0.274f - blue * 0.322f;
            var originalQ = red * 0.211f - green * 0.523f + blue * 0.312f;
            var blurredI = 0f;
            var blurredQ = 0f;
            for (var offset = -1; offset <= 1; offset++)
            {
                var sample = (y * width + Math.Clamp(x + offset, 0, width - 1)) * 3;
                var sampleRed = source[sample];
                var sampleGreen = source[sample + 1];
                var sampleBlue = source[sample + 2];
                blurredI += sampleRed * 0.596f - sampleGreen * 0.274f - sampleBlue * 0.322f;
                blurredQ += sampleRed * 0.211f - sampleGreen * 0.523f + sampleBlue * 0.312f;
            }
            var inPhase = Lerp(originalI, blurredI / 3f, amount);
            var quadrature = Lerp(originalQ, blurredQ / 3f, amount);
            colors[center] = Math.Clamp(luminance + 0.956f * inPhase + 0.621f * quadrature, 0f, 1f);
            colors[center + 1] = Math.Clamp(luminance - 0.272f * inPhase - 0.647f * quadrature, 0f, 1f);
            colors[center + 2] = Math.Clamp(luminance - 1.106f * inPhase + 1.703f * quadrature, 0f, 1f);
        }
    }

    private static float Lerp(float from, float to, float amount) => from + (to - from) * amount;
}
