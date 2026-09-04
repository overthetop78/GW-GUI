namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrixContrast
{
    internal const string Shader = "float filterDotMatrixContrast(float value,float amount){return clamp((value-.5)*(.5+clamp(amount,0.0,1.0)*2.5)+.5,0.0,1.0);}";

    internal static float Apply(float value, int amount) => Math.Clamp((value - .5f)
        * (.5f + Math.Clamp(amount, 0, 100) / 100f * 2.5f) + .5f, 0f, 1f);
}
