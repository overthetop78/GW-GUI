namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterFixedPixelPersistence
{
    internal const string Shader = """
        vec3 filterFixedPixelPersistence(vec3 current,vec3 previous,float intensity)
        { return clamp(max(current,previous*intensity),0.0,1.0); }
        """;

    internal static float Apply(float responded, float previous, int intensity) =>
        Math.Clamp(Math.Max(responded, previous * intensity / 100f), 0f, 1f);
}
