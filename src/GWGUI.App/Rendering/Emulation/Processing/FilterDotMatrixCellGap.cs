namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrixCellGap
{
    internal const string Shader =
        "float filterDotMatrixGap(float gap){return 1.0-.72*clamp(gap,0.0,1.0);}";

    internal static float Normalize(int value) => Math.Clamp(value, 0, 100) / 100f;
}
