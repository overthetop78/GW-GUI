using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Exploration.Contracts;
using GWGUI.MediaEngine.Exploration.Enums;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images.Reading;
using System.IO;

namespace GWGUI.MediaEngine.Exploration;

/// <summary>Loads one media document and explores its file-system content without presentation dependencies.</summary>
public sealed class MediaImageExplorationService(
    MediaImageReadingService reader,
    MediaExplorer explorer)
{
    public async Task<ExploredMediaImage> ExploreAsync(
        string path,
        string? requestedFormatId,
        CancellationToken cancellationToken = default,
        Action<MediaExplorationProgress>? reportProgress = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        reportProgress?.Invoke(new(
            MediaExplorationProgressStage.ReadingMedia,
            Path.GetFileName(path),
            58));

        var source = new MediaSourceDescriptor(path, [], RequestedFormatId: requestedFormatId);
        MediaImageDocument document;
        try
        {
            document = await reader.ReadAsync(source, cancellationToken).ConfigureAwait(false);
        }
        catch (NotSupportedException) when (requestedFormatId is not null)
        {
            document = await reader.ReadAsync(
                source with { RequestedFormatId = null },
                cancellationToken).ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        reportProgress?.Invoke(new(
            MediaExplorationProgressStage.ReadingFileSystem,
            document.FormatId,
            78,
            document.MediaKind));

        var explored = await explorer.ExploreAsync(document, cancellationToken).ConfigureAwait(false);
        reportProgress?.Invoke(new(
            MediaExplorationProgressStage.RecognizingFiles,
            FirstFileName(explored) ?? document.FormatId,
            94,
            document.MediaKind));
        return explored;
    }

    private static string? FirstFileName(ExploredMediaImage explored) =>
        explored.Volumes
            .SelectMany(volume => volume.FileSystem?.Entries ?? [])
            .SelectMany(EnumerateFiles)
            .Select(entry => entry.Name)
            .FirstOrDefault();

    private static IEnumerable<FileSystemEntry> EnumerateFiles(FileSystemEntry entry)
    {
        if (entry.Kind == FileSystemEntryKind.File) yield return entry;
        foreach (var child in entry.Children)
        {
            foreach (var descendant in EnumerateFiles(child)) yield return descendant;
        }
    }
}
