namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayBlackDepth
{
    internal const string Shader =
        "float filterSegmentBlackDepth(float blackDepth){return (1.0-clamp(blackDepth,0.0,1.0))*.08;}";

    internal static float Apply(int setting) =>
        (1f - Math.Clamp(setting, 0, 100) / 100f) * .08f;
}
