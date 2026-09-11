using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Partitioning;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Exploration;

/// <summary>Explores the volumes of an already recognized media image without recognizing its format again.</summary>
public sealed class MediaExplorer
{
    private readonly FileSystemRegistry fileSystems;
    private readonly MediaVolumeDetectorRegistry volumeDetectors;

    public MediaExplorer(FileSystemRegistry fileSystems)
        : this(fileSystems, new MediaVolumeDetectorRegistry([new WholeMediaVolumeDetector()]))
    {
    }

    public MediaExplorer(FileSystemRegistry fileSystems, MediaVolumeDetectorRegistry volumeDetectors)
    {
        ArgumentNullException.ThrowIfNull(fileSystems);
        ArgumentNullException.ThrowIfNull(volumeDetectors);
        this.fileSystems = fileSystems;
        this.volumeDetectors = volumeDetectors;
    }

    public ExploredMediaImage Explore(MediaImageDocument document)
        => ExploreAsync(document).AsTask().GetAwaiter().GetResult();

    public async ValueTask<ExploredMediaImage> ExploreAsync(
        MediaImageDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        var detection = await volumeDetectors.DetectAsync(document, cancellationToken).ConfigureAwait(false);
        var enrichedDocument = document.Volumes.SequenceEqual(detection.Volumes)
            ? document
            : new MediaImageDocument(
                document.Source,
                document.FormatId,
                document.MediaKind,
                document.Representation,
                detection.Volumes,
                document.Diagnostics,
                document.Metadata);
        var explored = detection.Volumes.Select(volume => fileSystems.Explore(enrichedDocument, volume)).ToArray();
        var diagnostics = document.Diagnostics.Concat(detection.Diagnostics).ToArray();
        return new(enrichedDocument, explored, diagnostics);
    }
}
