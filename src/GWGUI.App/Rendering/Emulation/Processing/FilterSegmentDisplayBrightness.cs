namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayBrightness
{
    internal const string Shader = "float filterSegmentBrightness(float emission,float brightness){return clamp(emission*(.2+clamp(brightness,0.0,1.0)*1.3),0.0,1.0);}";
    internal static float Apply(float emission, int brightness) => Math.Clamp(emission
        * (.2f + Math.Clamp(brightness, 0, 100) / 100f * 1.3f), 0f, 1f);
}
