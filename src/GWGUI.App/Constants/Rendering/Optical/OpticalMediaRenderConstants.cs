using SkiaSharp;

namespace GWGUI.App.Constants.Rendering.Optical;

internal static class OpticalMediaRenderConstants
{
    internal const float OuterMargin = 24f;
    internal const float InnerRadiusRatio = 0.18f;
    internal const float TrackGapDegrees = 0.3f;
    internal const float SelectionStrokeWidth = 3f;

    internal static SKColor BackgroundColor { get; } = new(9, 12, 16);
    internal static SKColor UnknownTrackColor { get; } = new(108, 119, 132);
    internal static SKColor DataTrackColor { get; } = new(48, 151, 214);
    internal static SKColor AudioTrackColor { get; } = new(183, 94, 204);
    internal static SKColor SelectionColor { get; } = SKColors.White;
}
