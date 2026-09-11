using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Interfaces.Exploration;

namespace GWGUI.MediaEngine.Exploration.Sequential;

/// <summary>Exposes decoded sequential media content as one explorable logical volume.</summary>
public sealed class SequentialContentVolumeDetector : IMediaVolumeDetector
{
    public ValueTask<MediaVolumeDetectionResult?> DetectAsync(
        MediaImageDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();

        if (document.MediaKind != MediaKind.Tape
            || document.Representation.RepresentationKind != MediaRepresentationKind.Sequential)
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
