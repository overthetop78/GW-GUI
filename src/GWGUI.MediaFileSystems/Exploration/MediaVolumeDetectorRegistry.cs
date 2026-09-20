using System.Collections.ObjectModel;
using MediaVolumeDetectionResult = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDetectionResult;
using IMediaImageDocument = global::GWGUI.MediaFileSystems.Interfaces.IMediaImageDocument;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

namespace GWGUI.MediaFileSystems.Exploration;

/// <summary>Selects the first registered volume detector that recognizes an already parsed media document.</summary>
public sealed class MediaVolumeDetectorRegistry
{
    private readonly IReadOnlyList<IMediaVolumeDetector> detectors;

    public MediaVolumeDetectorRegistry(IEnumerable<IMediaVolumeDetector> detectors)
    {
        ArgumentNullException.ThrowIfNull(detectors);
        var materialized = detectors.ToArray();
        if (materialized.Any(detector => detector is null))
            throw new ArgumentException("A volume detector cannot be null.", nameof(detectors));
        this.detectors = new ReadOnlyCollection<IMediaVolumeDetector>(materialized);
    }

    public IReadOnlyList<IMediaVolumeDetector> Detectors => detectors;

    public async ValueTask<MediaVolumeDetectionResult> DetectAsync(
        IMediaImageDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Volumes.Count > 0)
            return new MediaVolumeDetectionResult(document.Volumes, []);

        foreach (var detector in detectors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await detector.DetectAsync(document, cancellationToken).ConfigureAwait(false);
            if (result is not null) return result;
        }
        return new MediaVolumeDetectionResult(
            [],
            ["No registered detector can describe the media volumes."]);
    }
}
