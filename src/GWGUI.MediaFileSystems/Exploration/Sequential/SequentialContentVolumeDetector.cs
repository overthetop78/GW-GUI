using MediaVolumeOrigins = global::GWGUI.MediaFileSystems.Constants.MediaVolumeOrigins;
using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using MediaVolumeDetectionResult = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDetectionResult;
using IMediaImageDocument = global::GWGUI.MediaFileSystems.Interfaces.IMediaImageDocument;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

using System.IO;
using GWGUI.MediaFileSystems.Definitions;

namespace GWGUI.MediaFileSystems.Exploration.Sequential;

/// <summary>Exposes decoded sequential media content as one explorable logical volume.</summary>
public sealed class SequentialContentVolumeDetector : IMediaVolumeDetector
{
    public ValueTask<MediaVolumeDetectionResult?> DetectAsync(
        IMediaImageDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();

        if (!document.IsTape
            || document.Representation.SupportsRandomAccess
            || !document.Representation.SupportsSequentialAccess)
            return new ValueTask<MediaVolumeDetectionResult?>((MediaVolumeDetectionResult?)null);

        var length = document.Representation.LogicalLength
            ?? document.Source.KnownLength
            ?? GetSourceLength(document.Source.PrimaryPath);
        MediaVolumeDetectionResult result = length is null or <= 0
            ? new MediaVolumeDetectionResult(
                [],
                ["The sequential media representation does not expose a positive content length."])
            : new MediaVolumeDetectionResult(
                [new MediaVolumeDescriptor(
                    0,
                    length.Value,
                    MediaVolumeOrigins.SequentialContent,
                    fileSystemId: FileSystemIds.SequentialContent)],
                []);
        return new ValueTask<MediaVolumeDetectionResult?>(result);
    }

    private static long? GetSourceLength(string path)
        => File.Exists(path) ? new FileInfo(path).Length : null;
}
