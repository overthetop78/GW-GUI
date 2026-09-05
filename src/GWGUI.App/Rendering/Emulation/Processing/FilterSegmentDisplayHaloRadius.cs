namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayHaloRadius
{
    internal const string Shader =
        "float filterSegmentHaloFalloff(float distance,float radius){float extent=.025+clamp(radius,0.0,1.0)*.24;return exp(-max(distance,0.0)*4.0/extent);}";

    internal static float Apply(float distance, int setting)
    {
        var extent = .025f + Math.Clamp(setting, 0, 100) / 100f * .24f;
        return MathF.Exp(-MathF.Max(distance, 0f) * 4f / extent);
    }
}
