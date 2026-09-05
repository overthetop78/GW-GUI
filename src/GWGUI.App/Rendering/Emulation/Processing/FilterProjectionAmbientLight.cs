namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterProjectionAmbientLight
{
    internal const string Shader = "float projectionAmbientLight(float value,float setting){return mix(value,1.0,setting*.35);}";

    internal static float Apply(float value, float setting) =>
        value + (1f - value) * setting * .35f;
}

