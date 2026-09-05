namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterProjectionScreenTexture
{
    internal const string Shader = "float projectionScreenTexture(float value,vec2 pixel,float setting){vec2 p=mod(floor(pixel),4.0);float weave=(p.x<.5?.65:0.0)+(p.y<.5?.35:0.0);return value*(1.0-setting*.22*weave);}";

    internal static float Apply(float value, int x, int y, float setting) =>
        value * (1f - setting * .22f * (((x & 3)==0 ? .65f : 0f) + ((y & 3)==0 ? .35f : 0f)));
}

