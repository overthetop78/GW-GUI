namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVectorLineIntensity
{
    internal const string Shader = """
        vec3 filterVectorLineIntensity(vec3 color,float emission,float intensity)
        {
            float line=emission*intensity;
            return color+(vec3(1.0)-color)*line;
        }
        """;

    internal static void Apply(float[] colors, float[] emission, int setting)
    {
        var intensity = setting / 100f;
        for (var pixel = 0; pixel < emission.Length; pixel++)
        {
            var line = emission[pixel] * intensity;
            var index = pixel * 3;
            colors[index] += (1f - colors[index]) * line;
            colors[index + 1] += (1f - colors[index + 1]) * line;
            colors[index + 2] += (1f - colors[index + 2]) * line;
        }
    }
}
