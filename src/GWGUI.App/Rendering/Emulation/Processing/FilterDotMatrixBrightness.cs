namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrixBrightness
{
    internal const string Shader = "float filterDotMatrixBrightness(float value,float amount){return value*(.05+clamp(amount,0.0,1.0)*1.20);}";

    internal static float Apply(float value, int amount) => value
        * (.05f + Math.Clamp(amount, 0, 100) / 100f * 1.2f);
}
