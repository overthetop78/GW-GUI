namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrixDotSize
{
    internal const string Shader = "float filterDotMatrixRadius(float size,float gap){return .08+.42*clamp(size,0.0,1.0)*filterDotMatrixGap(gap);}";

    internal static float Radius(int size, int gap) => .08f + .42f
        * Math.Clamp(size, 0, 100) / 100f
        * (1f - .72f * FilterDotMatrixCellGap.Normalize(gap));
}
