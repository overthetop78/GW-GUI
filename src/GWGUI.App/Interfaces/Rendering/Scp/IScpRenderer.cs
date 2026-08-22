using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.MediaEngine.Containers.Scp;
using SkiaSharp;

namespace GWGUI.App.Interfaces.Rendering.Scp;

public interface IScpRenderer
{
    string? DecoderId { get; set; }
    void ClearCache();
    Task PrepareAsync(ScpImage image, int head, IProgress<ScpTrackPreparation>? progress = null, CancellationToken cancellationToken = default);
    void Render(SKCanvas canvas, ScpRenderRequest request);
}
