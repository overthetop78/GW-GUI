using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Exploration.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;
using GWGUI.MediaEngine.Images.Reading;
using System.IO;

namespace GWGUI.MediaEngine.Exploration;

/// <summary>Loads one media source once and derives every exploration result from the loaded document.</summary>
public sealed class MediaOpeningAnalysisService(
    MediaImageReadingService reader,
    DiskImageExplorer diskExplorer,
    MediaExplorer mediaExplorer)
{
    public async Task<MediaOpeningAnalysisResult> AnalyzeAsync(
        string path,
        string? requestedFormatId = null,
        IProgress<ScpExplorationProgress>? scpProgress = null,
        Action<MediaExplorationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        progress?.Invoke(new(
            MediaExplorationProgressStage.ReadingMedia,
            Path.GetFileName(path),
            12));

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
        progress?.Invoke(new(
            MediaExplorationProgressStage.ReadingFileSystem,
            document.FormatId,
            45,
            document.MediaKind));

        var diskTask = diskExplorer.ExploreAsync(
            document,
            requestedFormatId,
            scpProgress,
            cancellationToken);
        var mediaTask = mediaExplorer.ExploreAsync(document, cancellationToken).AsTask();
        await Task.WhenAll(diskTask, mediaTask).ConfigureAwait(false);

        progress?.Invoke(new(
            MediaExplorationProgressStage.RecognizingFiles,
            document.FormatId,
            94,
            document.MediaKind));
        return new(document, await diskTask.ConfigureAwait(false), await mediaTask.ConfigureAwait(false));
    }
}
