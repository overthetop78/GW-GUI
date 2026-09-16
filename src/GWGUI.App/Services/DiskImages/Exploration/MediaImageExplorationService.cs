using GWGUI.App.Constants.Localization;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Contracts;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Reading;
using System.IO;

namespace GWGUI.App.Services.DiskImages.Exploration;

internal sealed class MediaImageExplorationService(
    MediaImageReadingService? reader,
    MediaExplorer? explorer,
    Func<string, object[], string> localize)
{
    internal async Task<ExploredMediaImage?> ExploreAsync(
        string path,
        string? requestedFormatId,
        CancellationToken cancellationToken,
        Action<string, string, double>? reportProgress = null)
    {
        if (reader is null || explorer is null) return null;

        var isCassette = Path.GetExtension(path).Equals(DiskImageFileExtensions.Cas, StringComparison.OrdinalIgnoreCase);
        reportProgress?.Invoke(
            localize(
                isCassette
                    ? DiskImageResourceKeys.ExplorerLoadingTapeReading
                    : DiskImageResourceKeys.ExplorerLoadingMedia,
                []),
            Path.GetFileName(path),
            58);
        var source = new MediaSourceDescriptor(path, [], RequestedFormatId: requestedFormatId);
        MediaImageDocument document;
        try
        {
            document = await reader.ReadAsync(source, cancellationToken);
        }
        catch (NotSupportedException) when (requestedFormatId is not null)
        {
            document = await reader.ReadAsync(source with { RequestedFormatId = null }, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        reportProgress?.Invoke(
            localize(
                isCassette
                    ? DiskImageResourceKeys.ExplorerLoadingTapeDecoding
                    : DiskImageResourceKeys.ExplorerLoadingFileSystem,
                []),
            document.FormatId,
            78);
        var explored = await explorer.ExploreAsync(document, cancellationToken);
        var recognizedFiles = FileDescriptions(explored);
        reportProgress?.Invoke(
            localize(
                isCassette
                    ? DiskImageResourceKeys.ExplorerLoadingFileRecognition
                    : DiskImageResourceKeys.ExplorerLoadingContents,
                []),
            recognizedFiles.Count > 0 ? recognizedFiles[0] : explored.Document.FormatId,
            94);
        return explored;
    }

    internal static IReadOnlyList<string> FileDescriptions(ExploredMediaImage? explored)
    {
        if (explored?.Document.MediaKind != MediaKind.Tape) return [];
        var family = ExplorerFileIconClassifier.FamilyFor(
            explored.Document.FormatId,
            explored.Volumes.Select(volume => volume.FileSystem?.FileSystemId).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id)));
        return explored.Volumes
            .SelectMany(volume => volume.FileSystem?.Entries ?? [])
            .SelectMany(EnumerateFiles)
            .Select(entry => new ExplorerContentItem(entry, family))
            .Select(item => $"{item.Name} — {item.TypeText}")
            .ToArray();
    }

    private static IEnumerable<FileSystemEntry> EnumerateFiles(FileSystemEntry entry)
    {
        if (entry.Kind == FileSystemEntryKind.File) yield return entry;
        foreach (var child in entry.Children)
            foreach (var descendant in EnumerateFiles(child))
                yield return descendant;
    }
}
