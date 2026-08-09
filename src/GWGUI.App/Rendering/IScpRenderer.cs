using GWGUI.Scp;
using SkiaSharp;

namespace GWGUI.App.Rendering;

public enum DiskMediaKind { Unknown, ThreeInch, ThreeHalfDd, ThreeHalfHd, FiveQuarterDd, FiveQuarterHd, EightInch }

public sealed record ScpRenderRequest(
    ScpImage? Image,
    int Head,
    ScpTrack? SelectedTrack,
    int Width,
    int Height,
    SKPoint Center,
    float Zoom,
    string EmptySideText,
    string SideText,
    DiskMediaKind MediaKind = DiskMediaKind.Unknown);

public enum ScpTrackVisualState
{
    NormalFlux,
    ShortTransition,
    LongTransition,
    Header,
    DecodedData,
    Anomaly
}

public sealed record ScpTrackPreparation(
    int Cylinder,
    int Head,
    ScpTrackVisualState State,
    int ValidSectors = 0,
    int InvalidSectors = 0,
    int UnverifiedSectors = 0,
    bool HasFlux = true);

public interface IScpRenderer
{
    string? DecoderId { get; set; }
    void ClearCache();
    Task PrepareAsync(ScpImage image, int head, IProgress<ScpTrackPreparation>? progress = null, CancellationToken cancellationToken = default);
    void Render(SKCanvas canvas, ScpRenderRequest request);
}
