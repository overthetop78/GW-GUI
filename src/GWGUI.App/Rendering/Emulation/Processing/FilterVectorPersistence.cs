namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVectorPersistence
{
    internal const string Shader = """
        vec3 filterVectorPersistence(vec3 color,vec3 previous,float intensity)
        {
            return max(color,previous*intensity);
        }
        """;

    internal static void Apply(float[] colors, float[] previous, int setting)
    {
        if (setting <= 0) return;
        var persistence = setting / 100f;
        for (var index = 0; index < colors.Length; index++)
            colors[index] = Math.Max(colors[index], previous[index] * persistence);
    }
}
