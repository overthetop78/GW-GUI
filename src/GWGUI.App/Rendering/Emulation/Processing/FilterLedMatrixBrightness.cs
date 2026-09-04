namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterLedMatrixBrightness
{
    internal const string Shader = """
        vec3 filterLedMatrixBrightness(vec3 emission,float brightness)
        {return emission*clamp(brightness,0.0,1.0)*1.35;}
        """;

    internal static void Apply(float[] emission, int brightness)
    {
        var scale = Math.Clamp(brightness, 0, 100) / 100f * 1.35f;
        for (var index = 0; index < emission.Length; index++) emission[index] *= scale;
    }
}
