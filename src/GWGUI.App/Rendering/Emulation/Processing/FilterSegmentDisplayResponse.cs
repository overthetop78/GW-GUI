namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayResponse
{
    internal static float BlendFactor(int milliseconds, double elapsedMilliseconds) =>
        milliseconds <= 0 ? 1f : 1f - MathF.Exp(-(float)elapsedMilliseconds / milliseconds);
}
