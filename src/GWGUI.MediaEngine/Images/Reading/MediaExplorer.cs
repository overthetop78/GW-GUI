using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Contracts.Explorer;
using FileSystemsMediaExplorer = GWGUI.MediaFileSystems.Exploration.MediaExplorer;

namespace GWGUI.MediaEngine.Images.Reading;

/// <summary>Transmet le média décodé à MediaFileSystems et prépare le résultat remis à App.</summary>
public sealed class MediaExplorer(FileSystemsMediaExplorer fileSystems)
{
    public ExploredMediaImage Explore(MediaImageDocument document)
        => ExploreAsync(document).AsTask().GetAwaiter().GetResult();

    public async ValueTask<ExploredMediaImage> ExploreAsync(
        MediaImageDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        var result = await fileSystems.ExploreAsync(document, cancellationToken).ConfigureAwait(false);
        var enrichedDocument = document.FileSystemVolumes.SequenceEqual(result.Volumes)
            ? document
            : new MediaImageDocument(
                document.Source,
                document.FormatId,
                document.MediaKind,
                document.Representation,
                result.Volumes,
                document.Diagnostics,
                document.Metadata);
        var volumes = result.ExploredVolumes
            .Select(volume => new ExploredMediaVolume(
                volume.Descriptor,
                volume.ReaderId,
                volume.FileSystem is null
                    ? null
                    : FileSystemVolumeMapper.ConvertVolume(volume.FileSystem, volume.AnalyzedEntries),
                volume.Diagnostics))
            .ToArray();
        var diagnostics = document.Diagnostics.Concat(result.Diagnostics).ToArray();
        return new ExploredMediaImage(enrichedDocument, volumes, diagnostics, result.FirstFileName);
    }
}
