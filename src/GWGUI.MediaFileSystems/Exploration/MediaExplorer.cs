using System.IO;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Interfaces;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

namespace GWGUI.MediaFileSystems.Exploration;

/// <summary>Retrouve les volumes, dossiers et fichiers dans un média déjà décodé.</summary>
public sealed class MediaExplorer
{
    private readonly IReadOnlyList<IMediaFileSystemReader> readers;
    private readonly MediaVolumeDetectorRegistry volumeDetectors;

    public MediaExplorer(
        IEnumerable<IMediaFileSystemReader> readers,
        MediaVolumeDetectorRegistry volumeDetectors)
    {
        ArgumentNullException.ThrowIfNull(readers);
        ArgumentNullException.ThrowIfNull(volumeDetectors);
        var materialized = readers.ToArray();
        for (var index = 0; index < materialized.Length; index++)
        {
            if (materialized[index] is null) throw FileSystemRegistryExceptions.NullReader(index);
            if (string.IsNullOrWhiteSpace(materialized[index].Id))
                throw FileSystemRegistryExceptions.EmptyReaderId(index);
        }
        var duplicate = materialized.GroupBy(reader => reader.Id, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Skip(1).Any());
        if (duplicate is not null) throw FileSystemRegistryExceptions.DuplicateReaderId(duplicate.Key);
        this.readers = Array.AsReadOnly(materialized);
        this.volumeDetectors = volumeDetectors;
    }

    public IReadOnlyList<IMediaFileSystemReader> Readers => readers;

    public async ValueTask<MediaFileSystemExplorationResult> ExploreAsync(
        IMediaImageDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        var detection = await volumeDetectors.DetectAsync(document, cancellationToken).ConfigureAwait(false);
        var explored = new List<ExploredFileSystemVolume>();
        foreach (var volume in detection.Volumes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            explored.Add(ExploreVolume(document, volume));
        }
        return new MediaFileSystemExplorationResult(
            detection.Volumes, explored, detection.Diagnostics);
    }

    private ExploredFileSystemVolume ExploreVolume(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        IEnumerable<IMediaFileSystemReader> ordered = string.IsNullOrWhiteSpace(volume.FileSystemId)
            ? readers
            : readers.OrderByDescending(reader =>
                reader.Id.Equals(volume.FileSystemId, StringComparison.OrdinalIgnoreCase));
        var diagnostics = new List<string>();
        foreach (var reader in ordered)
        {
            if (!reader.CanRead(document, volume)) continue;
            try
            {
                return new ExploredFileSystemVolume(volume, reader.Id, reader.Read(document, volume), diagnostics);
            }
            catch (InvalidDataException exception)
            {
                diagnostics.Add($"{reader.Id}: {exception.Message}");
            }
        }
        return new ExploredFileSystemVolume(volume, null, null, diagnostics);
    }
}
