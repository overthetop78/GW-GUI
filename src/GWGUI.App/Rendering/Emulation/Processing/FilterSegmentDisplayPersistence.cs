namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayPersistence
{
    internal static float Decay(int milliseconds, double elapsedMilliseconds) =>
        milliseconds <= 0 ? 0f : MathF.Exp(-(float)elapsedMilliseconds / milliseconds);
}
