namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVfdHaloRadius
{
    internal const string Shader = """
        float filterVfdHaloRadius(float setting)
        {
            return mix(1.0,6.0,setting);
        }
        """;

    internal static float SourcePixels(int setting) => 1f + setting / 100f * 5f;
}
