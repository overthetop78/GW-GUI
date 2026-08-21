using GWGUI.MediaEngine.Containers.Scp;
using SkiaSharp;

namespace GWGUI.App.Rendering;

public interface IScpRenderer
{
    string? DecoderId { get; set; }
    void ClearCache();
    Task PrepareAsync(ScpImage image, int head, IProgress<ScpTrackPreparation>? progress = null, CancellationToken cancellationToken = default);
    void Render(SKCanvas canvas, ScpRenderRequest request);
}
