using GWGUI.App.Rendering.Sequential;
using GWGUI.App.Constants.DiskImages;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts.Explorer;
using System.Windows.Media;

namespace GWGUI.App.Services.DiskImages.Visualization;

internal sealed class CassetteLoadingPresenter(
    ExplorerSection explorer,
    VisualizerTabSection visualizer,
    MainWindowViewModel viewModel,
    TrackProgressStrip face0Progress,
    TrackProgressStrip face1Progress,
    MediaVisualizationController mediaVisualization,
    Action<string, string, double, bool> reportSharedProgress,
    Func<string, object[], string> localize)
{
    private static readonly TimeSpan MinimumSegmentPresentationInterval = TimeSpan.FromMilliseconds(4);

    internal async Task CompleteAsync(
        CancellationToken cancellationToken,
        string shownName,
        ExploredMediaImage? exploredMediaImage)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (mediaVisualization.SequentialRenderModel is not { } model || model.Segments.Count == 0)
        {
            reportSharedProgress(localize(DiskImageResourceKeys.VisualLoading, []), shownName, 100, false);
            return;
        }

        mediaVisualization.ConfigureSequentialProgress(model);
        var recognizedFiles = FileDescriptions(exploredMediaImage);
        var rowSize = (model.Segments.Count + MediaVisualizationLayoutConstants.SegmentProgressPageCount - 1)
                      / MediaVisualizationLayoutConstants.SegmentProgressPageCount;
        var lastPresentation = System.Diagnostics.Stopwatch.GetTimestamp();
        for (var index = 0; index < model.Segments.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(lastPresentation);
            var delay = MinimumSegmentPresentationInterval - elapsed;
            if (delay > TimeSpan.Zero) await Task.Delay(delay, cancellationToken);

            var row = index < rowSize
                ? MediaVisualizationLayoutConstants.SegmentProgressPageOne
                : MediaVisualizationLayoutConstants.SegmentProgressPageTwo;
            var position = row == MediaVisualizationLayoutConstants.SegmentProgressPageOne
                ? index
                : index - rowSize;
            var segment = model.Segments[index];
            var color = SkiaSequentialMediaRenderer.ColorFor(segment);
            (row == MediaVisualizationLayoutConstants.SegmentProgressPageOne ? face0Progress : face1Progress).SetColor(
                position,
                Color.FromRgb(color.Red, color.Green, color.Blue));

            var completed = index + 1;
            var value = completed * 100d / model.Segments.Count;
            var stage = localize(DiskImageResourceKeys.ExplorerLoadingTapeReading, []);
            var detail = localize(DiskImageResourceKeys.ExplorerLoadingTapeRecord, [completed, model.Segments.Count]);
            if (recognizedFiles.Count > 0)
            {
                var recognizedIndex = Math.Min(recognizedFiles.Count - 1, index * recognizedFiles.Count / model.Segments.Count);
                detail = $"{detail} · {recognizedFiles[recognizedIndex]}";
            }
            viewModel.ProgressValue = value;
            viewModel.ProgressText = string.Empty;
            explorer.SetLoadingProgress(stage, detail, value);
            visualizer.SetRecognitionProgress(true, stage, detail, value);
            lastPresentation = System.Diagnostics.Stopwatch.GetTimestamp();
        }
    }

    private static IReadOnlyList<string> FileDescriptions(ExploredMediaImage? explored)
    {
        if (explored?.Document.MediaKind != MediaKind.Tape) return [];
        return explored.Volumes
            .SelectMany(volume => volume.FileSystem?.Entries ?? [])
            .SelectMany(EnumerateFiles)
            .Select(entry => new ExplorerContentItem(entry))
            .Select(item => $"{item.Name} — {item.TypeText}")
            .ToArray();
    }

    private static IEnumerable<FileSystemEntry> EnumerateFiles(FileSystemEntry entry)
    {
        if (entry.Kind == FileSystemEntryKind.File) yield return entry;
        foreach (var child in entry.Children)
        {
            foreach (var descendant in EnumerateFiles(child)) yield return descendant;
        }
    }
}
