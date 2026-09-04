namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterPlasmaAutomaticBrightnessLimiter
{
    internal const string Shader = """
        vec3 filterPlasmaAutomaticBrightnessLimiter(vec3 color,float intensity,float averageLuminance)
        {
            if(intensity<=0.0)return color;
            float load=smoothstep(.28,.78,averageLuminance);
            return color*(1.0-intensity*.42*load);
        }
        """;

    internal static void Apply(float[] colors, int setting)
    {
        if (setting <= 0 || colors.Length == 0) return;
        var average = 0f;
        for (var index = 0; index < colors.Length; index += 3)
            average += colors[index] * 0.2126f + colors[index + 1] * 0.7152f
                + colors[index + 2] * 0.0722f;
        average /= colors.Length / 3f;
        var load = SmoothStep(0.28f, 0.78f, average);
        var gain = 1f - setting / 100f * 0.42f * load;
        for (var index = 0; index < colors.Length; index++) colors[index] *= gain;
    }

    internal static float MeasureBgra(ReadOnlySpan<byte> pixels)
    {
        var pixelCount = pixels.Length / 4;
        if (pixelCount == 0) return 0f;
        var stride = Math.Max(1, pixelCount / 4096);
        var sum = 0f;
        var samples = 0;
        for (var pixel = 0; pixel < pixelCount; pixel += stride)
        {
            var index = pixel * 4;
            var red = pixels[index + 2] / 255f;
            var green = pixels[index + 1] / 255f;
            var blue = pixels[index] / 255f;
            sum += red * red * 0.2126f + green * green * 0.7152f + blue * blue * 0.0722f;
            samples++;
        }
        return sum / Math.Max(1, samples);
    }

    private static float SmoothStep(float low, float high, float value)
    {
        var position = Math.Clamp((value - low) / (high - low), 0f, 1f);
        return position * position * (3f - 2f * position);
    }
}
