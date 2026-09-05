namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayCellSize
{
    internal const string Shader = "float filterSegmentCellWidth(float value){return 6.0+floor(clamp(value,0.0,1.0)*26.0+.5);}";

    internal static int Width(int value) => 6 + (int)MathF.Round(Math.Clamp(value, 0, 100) / 100f * 26f);
    internal static int Height(int value) => Math.Max(9, (int)MathF.Round(Width(value) * 1.55f));
}
