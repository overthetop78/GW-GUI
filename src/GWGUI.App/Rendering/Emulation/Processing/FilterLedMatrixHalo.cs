namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterLedMatrixHalo
{
    internal const string Shader = """
        float filterLedMatrixHalo(float distance,float gap,float radius,float intensity)
        {
            float coreRadius=.5*(1.0-clamp(gap,0.0,1.0)*.86);
            float extent=.04+clamp(radius,0.0,1.0)*.9;
            return exp(-max(0.0,distance-coreRadius)*4.0/extent)*clamp(intensity,0.0,1.0);
        }
        """;

    internal static float Intensity(int value) => Math.Clamp(value, 0, 100) / 100f;
}
