namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayThickness
{
    internal const string Shader = "float filterSegmentThickness(float value){return .018+clamp(value,0.0,1.0)*.105;}";
    internal static float Radius(int value) => .018f + Math.Clamp(value, 0, 100) / 100f * .105f;
}
