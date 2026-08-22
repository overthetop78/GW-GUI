using SkiaSharp;

namespace GWGUI.App.Rendering.Scp;

public sealed partial class SkiaScpRenderer
{
    private sealed record PreparedScpArc(float Start, float Sweep, SKColor Color);
}
