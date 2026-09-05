namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayGlow
{
    internal const string Shader =
        "float filterSegmentGlow(float value,float intensity){return value*clamp(intensity,0.0,1.0);}";

    internal static float Apply(float value, int setting) =>
        value * Math.Clamp(setting, 0, 100) / 100f;
}
