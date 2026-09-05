namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayContrast
{
    internal const string Shader =
        "float filterSegmentContrast(float thresholded,float contrast){float gain=.5+clamp(contrast,0.0,1.0)*3.5;return clamp(thresholded*gain+.5,0.0,1.0);}";

    internal static float Apply(float thresholded, int setting)
    {
        var gain = .5f + Math.Clamp(setting, 0, 100) / 100f * 3.5f;
        return Math.Clamp(thresholded * gain + .5f, 0f, 1f);
    }
}
