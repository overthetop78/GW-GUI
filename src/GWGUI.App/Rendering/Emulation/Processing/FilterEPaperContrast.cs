namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterEPaperContrast
{
    internal const string Shader = "float filterEPaperContrast(float value,float setting){float contrast=.6+clamp(setting,0.0,1.0)*1.8;return clamp((value-.5)*contrast+.5,0.0,1.0);}";
    internal static float Apply(float value, int setting)
    {
        var contrast = .6f + Math.Clamp(setting, 0, 100) / 100f * 1.8f;
        return Math.Clamp((value - .5f) * contrast + .5f, 0f, 1f);
    }
}
