namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVfdPhosphorIntensity
{
    internal const string Shader = """
        float filterVfdPhosphorIntensity(float emission,float intensity)
        {
            return clamp(emission*intensity*1.8,0.0,1.0);
        }
        """;

    internal static void Apply(float[] emission, int setting)
    {
        var scale = setting / 100f * 1.8f;
        for (var index = 0; index < emission.Length; index++)
            emission[index] = Math.Clamp(emission[index] * scale, 0f, 1f);
    }
}
