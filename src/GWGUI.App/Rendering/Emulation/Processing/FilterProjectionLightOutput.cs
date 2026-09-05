namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterProjectionLightOutput
{
    internal const string Shader = "float projectionLightOutput(float value,float setting){return clamp(value*(.25+setting*1.5),0.0,1.0);}";

    internal static float Apply(float value, float setting) =>
        Math.Clamp(value * (.25f + setting * 1.5f), 0f, 1f);
}

