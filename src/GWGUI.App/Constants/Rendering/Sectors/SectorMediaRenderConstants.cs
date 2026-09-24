using SkiaSharp;

namespace GWGUI.App.Constants.Rendering.Sectors;

internal static class SectorMediaRenderConstants
{
    internal const float InnerRadiusRatio = 0.22f;
    internal const float OuterMargin = 18f;
    internal const float MinimumSweepDegrees = 0.12f;
    internal const float SectorGapDegrees = 0.35f;
    internal const float SelectionStrokeWidth = 2.5f;
    internal const float LogicalGridMargin = 18f;
    internal const float LogicalGridGap = 1f;

    internal static SKColor BackgroundColor { get; } = new(239, 244, 247);
    internal static SKColor PendingColor { get; } = new(43, 32, 25);
    internal static SKColor PendingTrackBoundaryColor { get; } = new(8, 7, 6, 220);
    internal static SKColor WithDataColor { get; } = new(45, 176, 100);
    internal static SKColor WithoutDataColor { get; } = new(74, 83, 94);
    internal static SKColor DegradedColor { get; } = new(224, 151, 47);
    internal static SKColor DeadColor { get; } = new(207, 67, 67);
    internal static SKColor SelectionColor { get; } = SKColors.White;
}
