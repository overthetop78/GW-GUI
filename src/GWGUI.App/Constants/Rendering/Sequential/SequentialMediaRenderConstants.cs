using SkiaSharp;

namespace GWGUI.App.Constants.Rendering.Sequential;

internal static class SequentialMediaRenderConstants
{
    internal const int DefaultWrappedLineCount = 6;
    internal const int MaximumWrappedLineCount = 32;
    internal const float OuterMargin = 24f;
    internal const float LaneGap = 8f;
    internal const float SelectionStrokeWidth = 2f;

    internal static SKColor BackgroundColor { get; } = new(21, 26, 31);
    internal static SKColor TapeColor { get; } = new(45, 52, 59);
    internal static SKColor UnknownColor { get; } = new(108, 119, 132);
    internal static SKColor SignalColor { get; } = new(57, 68, 78);
    internal static SKColor SilenceColor { get; } = new(194, 202, 208);
    internal static SKColor DecodedBlockColor { get; } = new(47, 166, 91);
    internal static SKColor ErrorColor { get; } = new(207, 55, 62);
    internal static SKColor SelectionColor { get; } = SKColors.White;
}
