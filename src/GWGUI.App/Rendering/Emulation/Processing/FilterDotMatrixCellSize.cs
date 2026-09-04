namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterDotMatrixCellSize
{
    internal const string Shader = "float filterDotMatrixPitch(float size){return 1.0+floor(clamp(size,0.0,1.0)*15.0+.5);}";

    internal static int Pitch(int value) => 1 + (int)MathF.Round(
        Math.Clamp(value, 0, 100) / 100f * 15f);
}
