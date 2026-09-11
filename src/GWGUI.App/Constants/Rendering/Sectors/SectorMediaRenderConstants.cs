using SkiaSharp;

namespace GWGUI.App.Constants.Rendering.Sectors;

internal static class SectorMediaRenderConstants
{
    internal const float InnerRadiusRatio = 0.22f;
    internal const float OuterMargin = 18f;
    internal const float MinimumSweepDegrees = 0.12f;
    internal const float SectorGapDegrees = 0.35f;
    internal const float SelectionStrokeWidth = 2.5f;

    internal static SKColor BackgroundColor { get; } = new(9, 12, 16);
    internal static SKColor AvailableColor { get; } = new(45, 176, 100);
    internal static SKColor MissingColor { get; } = new(94, 103, 114);
    internal static SKColor IntegrityUnknownColor { get; } = new(224, 151, 47);
    internal static SKColor IntegrityInvalidColor { get; } = new(207, 67, 67);
    internal static SKColor SelectionColor { get; } = SKColors.White;
}
