namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVectorLineDetection
{
    internal const string Shader = """
        float filterVectorLineDetection(float gradientX,float gradientY,float threshold)
        {
            float magnitude=clamp(length(vec2(gradientX,gradientY))/4.0,0.0,1.0);
            return smoothstep(threshold,min(1.0,threshold+0.10),magnitude);
        }
        """;

    private const float RedLuminance = 0.2126f;
    private const float GreenLuminance = 0.7152f;
    private const float BlueLuminance = 0.0722f;

    internal static float[] Detect(float[] colors, int width, int height, int setting)
    {
        var luminance = new float[width * height];
        for (var pixel = 0; pixel < luminance.Length; pixel++)
        {
            var index = pixel * 3;
            luminance[pixel] = colors[index] * RedLuminance
                + colors[index + 1] * GreenLuminance
                + colors[index + 2] * BlueLuminance;
        }
        var emission = new float[width * height];
        var threshold = setting / 100f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var gradientX = -Sample(luminance, width, height, x - 1, y - 1)
                - 2f * Sample(luminance, width, height, x - 1, y)
                - Sample(luminance, width, height, x - 1, y + 1)
                + Sample(luminance, width, height, x + 1, y - 1)
                + 2f * Sample(luminance, width, height, x + 1, y)
                + Sample(luminance, width, height, x + 1, y + 1);
            var gradientY = -Sample(luminance, width, height, x - 1, y - 1)
                - 2f * Sample(luminance, width, height, x, y - 1)
                - Sample(luminance, width, height, x + 1, y - 1)
                + Sample(luminance, width, height, x - 1, y + 1)
                + 2f * Sample(luminance, width, height, x, y + 1)
                + Sample(luminance, width, height, x + 1, y + 1);
            var magnitude = Math.Clamp(MathF.Sqrt(
                gradientX * gradientX + gradientY * gradientY) / 4f, 0f, 1f);
            emission[y * width + x] = SmoothStep(threshold,
                Math.Min(1f, threshold + 0.1f), magnitude);
        }
        return emission;
    }

    private static float Sample(float[] values, int width, int height, int x, int y) =>
        values[Math.Clamp(y, 0, height - 1) * width + Math.Clamp(x, 0, width - 1)];

    private static float SmoothStep(float start, float end, float value)
    {
        if (end <= start) return value >= end ? 1f : 0f;
        var amount = Math.Clamp((value - start) / (end - start), 0f, 1f);
        return amount * amount * (3f - 2f * amount);
    }
}
