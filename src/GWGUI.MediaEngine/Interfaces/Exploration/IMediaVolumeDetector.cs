using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Interfaces.Exploration;

/// <summary>Detects logical volumes in an already recognized media document.</summary>
public interface IMediaVolumeDetector
{
    ValueTask<MediaVolumeDetectionResult?> DetectAsync(
        MediaImageDocument document,
        CancellationToken cancellationToken = default);
}
