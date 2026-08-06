using GWGUI.Scp;
using SkiaSharp;

namespace GWGUI.App.Rendering;

public sealed record ScpRenderRequest(
    ScpImage? Image,
    int Head,
    ScpTrack? SelectedTrack,
    int Width,
    int Height,
    SKPoint Center,
    float Zoom,
    string EmptySideText,
    string SideText);

public interface IScpRenderer
{
    string? DecoderId { get; set; }
    void ClearCache();
    Task PrepareAsync(ScpImage image, int head, IProgress<int>? progress = null, CancellationToken cancellationToken = default);
    void Render(SKCanvas canvas, ScpRenderRequest request);
}
