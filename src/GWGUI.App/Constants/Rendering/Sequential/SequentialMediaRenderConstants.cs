using SkiaSharp;

namespace GWGUI.App.Constants.Rendering.Sequential;

internal static class SequentialMediaRenderConstants
{
    internal const int DefaultWrappedLineCount = 6;
    internal const int MaximumWrappedLineCount = 32;
    internal const float OuterMargin = 16f;
    internal const float LaneGap = 4f;
    internal const float SelectionStrokeWidth = 2f;

    internal static SKColor BackgroundColor { get; } = new(9, 12, 16);
    internal static SKColor UnknownColor { get; } = new(108, 119, 132);
    internal static SKColor SignalColor { get; } = new(48, 151, 214);
    internal static SKColor SilenceColor { get; } = new(65, 72, 82);
    internal static SKColor DecodedBlockColor { get; } = new(45, 176, 100);
    internal static SKColor ErrorColor { get; } = new(207, 67, 67);
    internal static SKColor SelectionColor { get; } = SKColors.White;
}
