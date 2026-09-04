namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterPlasmaGammaResponse
{
    internal const string Shader = """
        vec3 filterPlasmaGammaResponse(vec3 color,float intensity)
        { return intensity<=0.0?color:pow(clamp(color,0.0,1.0),vec3(1.0+intensity*.32)); }
        """;

    internal static void Apply(float[] colors, int setting)
    {
        if (setting <= 0) return;
        var exponent = 1f + setting / 100f * 0.32f;
        for (var index = 0; index < colors.Length; index++)
            colors[index] = MathF.Pow(Math.Clamp(colors[index], 0f, 1f), exponent);
    }
}
