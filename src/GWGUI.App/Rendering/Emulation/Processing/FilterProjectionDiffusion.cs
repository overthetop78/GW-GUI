namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterProjectionDiffusion
{
    internal const string Shader = "float projectionDiffusion(float value,float average,float setting){return clamp(value+average*setting*.35,0.0,1.0);}";

    internal static float Apply(float value, float average, float setting) =>
        Math.Clamp(value + average * setting * .35f, 0f, 1f);
}

