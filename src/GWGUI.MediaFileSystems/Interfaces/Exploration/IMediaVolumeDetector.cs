using MediaVolumeDetectionResult = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDetectionResult;

namespace GWGUI.MediaFileSystems.Interfaces.Exploration;

/// <summary>Détecte les volumes logiques d'un média déjà reconnu.</summary>
public interface IMediaVolumeDetector
{
    ValueTask<MediaVolumeDetectionResult?> DetectAsync(
        IMediaImageDocument document,
        CancellationToken cancellationToken = default);
}
