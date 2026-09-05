namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterProjectionOpticalBlur
{
    internal const string Shader = "float projectionOpticalBlur(float value,float average,float setting){return mix(value,average,setting);}";

    internal static float Apply(float value, float average, float setting) =>
        value + (average - value) * setting;
}

