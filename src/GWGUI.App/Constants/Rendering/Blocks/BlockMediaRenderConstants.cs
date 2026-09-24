using SkiaSharp;

namespace GWGUI.App.Constants.Rendering.Blocks;

internal static class BlockMediaRenderConstants
{
    internal const int MaximumRenderedRanges = 2048;
    internal const float OuterMargin = 24f;
    internal const float InnerRadiusRatio = 0.28f;
    internal const float SelectionStrokeWidth = 3f;

    internal static SKColor BackgroundColor { get; } = new(9, 12, 16);
    internal static SKColor AvailableColor { get; } = new(45, 176, 100);
    internal static SKColor ReservedColor { get; } = new(91, 116, 191);
    internal static SKColor AllocatedColor { get; } = new(48, 151, 214);
    internal static SKColor FreeColor { get; } = new(132, 191, 103);
    internal static SKColor UnknownColor { get; } = new(94, 103, 114);
    internal static SKColor SelectionColor { get; } = SKColors.White;
}
