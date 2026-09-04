namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrixHalo
{
    internal const string Shader = "float filterDotMatrixHalo(float distance,float radius,float amount){float outside=max(0.0,distance-radius);return exp(-outside*8.0)*clamp(amount,0.0,1.0)*.85;}";

    internal static float Apply(float distance, float radius, int amount) =>
        MathF.Exp(-MathF.Max(0f, distance - radius) * 8f)
        * Math.Clamp(amount, 0, 100) / 100f * .85f;
}
