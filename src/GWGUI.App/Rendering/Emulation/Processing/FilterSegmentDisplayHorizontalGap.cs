namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayHorizontalGap
{
    internal const string Shader =
        "float filterSegmentHorizontalGap(float value,float gap){float scale=1.0-clamp(gap,0.0,1.0)*.42;return (value-.5)/scale+.5;}";

    internal static float Apply(float value, int setting) =>
        (value - .5f) / (1f - Math.Clamp(setting, 0, 100) / 100f * .42f) + .5f;
}
