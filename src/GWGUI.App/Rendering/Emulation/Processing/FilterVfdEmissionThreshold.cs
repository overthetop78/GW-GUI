namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVfdEmissionThreshold
{
    internal const string Shader = """
        float filterVfdEmissionThreshold(float luminance,float setting)
        {
            float threshold=setting*.85;
            return smoothstep(threshold,min(1.0,threshold+.08),luminance);
        }
        """;

    internal static float[] Extract(float[] colors, int setting)
    {
        var result = new float[colors.Length / 3];
        var threshold = setting / 100f * 0.85f;
        var upper = Math.Min(1f, threshold + 0.08f);
        for (var pixel = 0; pixel < result.Length; pixel++)
        {
            var index = pixel * 3;
            var luminance = colors[index] * 0.2126f + colors[index + 1] * 0.7152f
                + colors[index + 2] * 0.0722f;
            var value = Math.Clamp((luminance - threshold) / Math.Max(0.001f, upper - threshold), 0f, 1f);
            result[pixel] = value * value * (3f - 2f * value);
        }
        return result;
    }
}
