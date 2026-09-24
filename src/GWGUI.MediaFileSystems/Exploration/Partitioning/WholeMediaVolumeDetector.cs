using MediaVolumeOrigins = global::GWGUI.MediaFileSystems.Constants.MediaVolumeOrigins;
using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using MediaVolumeDetectionResult = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDetectionResult;
using IMediaImageDocument = global::GWGUI.MediaFileSystems.Interfaces.IMediaImageDocument;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

using System.IO;

namespace GWGUI.MediaFileSystems.Exploration.Partitioning;

/// <summary>Exposes the complete addressable media as one direct volume after partition detectors decline it.</summary>
public sealed class WholeMediaVolumeDetector : IMediaVolumeDetector
{
    public ValueTask<MediaVolumeDetectionResult?> DetectAsync(
        IMediaImageDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();
        var length = document.Representation.LogicalLength
            ?? document.Source.KnownLength
            ?? GetSourceLength(document.Source.PrimaryPath);
        MediaVolumeDetectionResult? result = length is null or <= 0
            ? new MediaVolumeDetectionResult(
                [],
                ["The media representation does not expose an addressable volume length."])
            : new MediaVolumeDetectionResult(
                [new MediaVolumeDescriptor(
                    0,
                    length.Value,
                    MediaVolumeOrigins.DirectVolume)],
                []);
        return new ValueTask<MediaVolumeDetectionResult?>(result);
    }

    private static long? GetSourceLength(string path)
        => File.Exists(path) ? new FileInfo(path).Length : null;
}
