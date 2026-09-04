namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVectorHaloRadius
{
    internal const string Shader = """
        float filterVectorHaloRadius(float radius)
        {
            return mix(1.0,6.0,radius);
        }
        """;

    internal static int Pixels(int setting) => 1 + (int)MathF.Round(setting / 100f * 5f);
}
