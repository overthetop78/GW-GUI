using GWGUI.App.Rendering.Sequential;
using GWGUI.App.Constants.DiskImages;
using GWGUI.App.Constants.Localization;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.MediaEngine.Exploration.Results;
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
        long generation,
        Func<long> currentGeneration,
        string shownName,
        ExploredMediaImage? exploredMediaImage)
    {
        if (mediaVisualization.SequentialRenderModel is not { } model || model.Segments.Count == 0)
        {
            reportSharedProgress(localize(DiskImageResourceKeys.VisualLoading, []), shownName, 100, false);
            return;
        }

        mediaVisualization.ConfigureSequentialProgress(model);
        var recognizedFiles = Exploration.MediaImageExplorationService.FileDescriptions(exploredMediaImage);
        var rowSize = (model.Segments.Count + MediaVisualizationLayoutConstants.SegmentProgressPageCount - 1)
                      / MediaVisualizationLayoutConstants.SegmentProgressPageCount;
        var lastPresentation = System.Diagnostics.Stopwatch.GetTimestamp();
        for (var index = 0; index < model.Segments.Count; index++)
        {
            if (generation != currentGeneration()) return;
            var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(lastPresentation);
            var delay = MinimumSegmentPresentationInterval - elapsed;
            if (delay > TimeSpan.Zero) await Task.Delay(delay);

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
}
