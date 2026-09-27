using SkiaSharp;

namespace GWGUI.App.Constants.Rendering.Blocks;

internal static class BlockMediaRenderConstants
{
    internal const int MaximumRenderedRanges = 2048;
    internal const float OuterMargin = 24f;
    internal const float InnerRadiusRatio = 0.28f;
    internal const float SelectionStrokeWidth = 3f;
    internal const float CartridgeAspectRatio = 1.3f;
    internal const float CartridgeMaximumWidthRatio = .68f;
    internal const float CartridgeMaximumHeightRatio = .82f;
    internal const int CartridgeMaximumColumns = 8;
    internal const float CartridgeCellGap = 2f;
    internal const float CartridgeCellRadius = 3f;
    internal const float CartridgeOutlineWidth = 3f;
    internal const float CartridgeCornerRadius = 18f;

    internal static SKColor BackgroundColor { get; } = new(9, 12, 16);
    internal static SKColor AvailableColor { get; } = new(45, 176, 100);
    internal static SKColor ReservedColor { get; } = new(91, 116, 191);
    internal static SKColor AllocatedColor { get; } = new(48, 151, 214);
    internal static SKColor FreeColor { get; } = new(132, 191, 103);
    internal static SKColor UnknownColor { get; } = new(94, 103, 114);
    internal static SKColor SelectionColor { get; } = SKColors.White;
    internal static SKColor CartridgeBodyColor { get; } = new(36, 43, 51);
    internal static SKColor CartridgeOutlineColor { get; } = new(112, 124, 137);
    internal static SKColor CartridgeTextColor { get; } = new(245, 247, 250);
    internal static SKColor CartridgePanelColor { get; } = new(20, 25, 30);
    internal static SKColor CartridgeConnectorColor { get; } = new(198, 151, 48);
}
