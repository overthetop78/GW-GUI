namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayActivationThreshold
{
    internal const string Shader =
        "float filterSegmentThreshold(float luminance,float threshold){return luminance-clamp(threshold,0.0,1.0);}";

    internal static float Apply(float luminance, int setting) =>
        luminance - Math.Clamp(setting, 0, 100) / 100f;
}
