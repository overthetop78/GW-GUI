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

public enum ScpTrackVisualState
{
    NormalFlux,
    ShortTransition,
    LongTransition,
    Header,
    DecodedData,
    Anomaly
}

public sealed record ScpTrackPreparation(int Cylinder, int Head, ScpTrackVisualState State);

public interface IScpRenderer
{
    string? DecoderId { get; set; }
    void ClearCache();
    Task PrepareAsync(ScpImage image, int head, IProgress<ScpTrackPreparation>? progress = null, CancellationToken cancellationToken = default);
    void Render(SKCanvas canvas, ScpRenderRequest request);
}
