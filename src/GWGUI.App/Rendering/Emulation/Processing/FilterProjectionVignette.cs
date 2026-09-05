namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterProjectionVignette
{
    internal const string Shader = "float projectionVignette(float value,vec2 uv,float setting){vec2 p=uv-.5;return value*(1.0-setting*.75*clamp(dot(p,p)*2.0,0.0,1.0));}";

    internal static float Apply(float value, float u, float v, float setting) =>
        value * (1f - setting * .75f * Math.Clamp(((u-.5f)*(u-.5f)+(v-.5f)*(v-.5f))*2f,0f,1f));
}

