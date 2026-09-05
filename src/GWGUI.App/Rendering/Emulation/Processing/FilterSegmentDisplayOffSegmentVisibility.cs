namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayOffSegmentVisibility
{
    internal const string Shader =
        "float filterSegmentOffVisibility(float activation,float visibility){return max(activation,clamp(visibility,0.0,1.0));}";

    internal static float Apply(float activation, int setting) =>
        MathF.Max(activation, Math.Clamp(setting, 0, 100) / 100f);
}
