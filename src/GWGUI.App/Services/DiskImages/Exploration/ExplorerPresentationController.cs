using GWGUI.App.Constants.Localization;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Contracts.Explorer;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;

namespace GWGUI.App.Services.DiskImages.Exploration;

internal sealed class ExplorerPresentationController
{
    private readonly ExplorerSection _explorer;
    private readonly VisualizerTabSection _visualizer;
    private readonly Func<string, string?, IProgress<ScpExplorationProgress>?, Action<MediaExplorationProgress>?, CancellationToken, Task<MediaOpeningAnalysisResult>> _analyze;
    private readonly DiskImageCancellationScope _cancellation;
    private readonly Action _applyClassification;
    private readonly Action<ExploredDiskImage> _rememberDiskImage;
    private readonly Action<ExploredMediaImage?> _setMediaImage;
    private readonly Action<Exception, string, string, string> _showError;
    private readonly Func<string, object[], string> _localize;

    internal ExplorerPresentationController(
        ExplorerSection explorer,
        VisualizerTabSection visualizer,
        Func<string, string?, IProgress<ScpExplorationProgress>?, Action<MediaExplorationProgress>?, CancellationToken, Task<MediaOpeningAnalysisResult>> analyze,
        DiskImageCancellationScope cancellation,
        Action applyClassification,
        Action<ExploredDiskImage> rememberDiskImage,
        Action<ExploredMediaImage?> setMediaImage,
        Action<Exception, string, string, string> showError,
        Func<string, object[], string> localize)
    {
        _explorer = explorer;
        _visualizer = visualizer;
        _analyze = analyze;
        _cancellation = cancellation;
        _applyClassification = applyClassification;
        _rememberDiskImage = rememberDiskImage;
        _setMediaImage = setMediaImage;
        _showError = showError;
        _localize = localize;
    }

    internal string? Path { get; private set; }
    internal ExploredDiskImage? CurrentImage { get; set; }
    internal MediaOpeningAnalysisResult? CurrentOpeningResult { get; private set; }

    internal Task<ExploredDiskImage?> LoadAsync(string path, bool newImage = true) =>
        LoadAsync(path, newImage, null);

    internal async Task<ExploredDiskImage?> LoadAsync(
        string path,
        bool newImage,
        Action<string, string, double, bool>? sharedProgress)
    {
        var cancellation = _cancellation.BeginExplorer();
        var cancellationToken = cancellation.Token;
        Path = path;
        _explorer.Clear(path, newImage);
        _explorer.SetLoading(true);
        var requestedFormat = newImage ? _explorer.FormatIdForNewImage : _explorer.SelectedFormatId;
        try
        {
            ReportProgress(DiskImageResourceKeys.ExplorerLoadingRecognition, System.IO.Path.GetFileName(path), 12, sharedProgress);
            var openingResult = SelectCachedInterpretation(path, newImage, requestedFormat)
                ?? await AnalyzeAsync(path, requestedFormat, cancellationToken, sharedProgress);
            cancellationToken.ThrowIfCancellationRequested();
            if (!_cancellation.IsCurrentExplorer(cancellation)) return null;

            var document = openingResult.DiskExploration;
            ReportProgress(
                DiskImageResourceKeys.ExplorerLoadingFormatFound,
                document.PrimaryFormatId,
                document.ScpImage is not null ? 90d : 45d,
                sharedProgress);
            cancellationToken.ThrowIfCancellationRequested();
            if (!_cancellation.IsCurrentExplorer(cancellation)) return null;

            CurrentOpeningResult = openingResult;
            CurrentImage = document;
            _rememberDiskImage(document);
            var mediaImage = openingResult.MediaExploration;
            _setMediaImage(mediaImage);
            if (mediaImage is not null) _explorer.Display(mediaImage);
            else _explorer.Display(document);

            if (mediaImage is not null)
            {
                var mediaFormatId = mediaImage.Document.FormatId;
                _visualizer.Header.ApplyDetection(mediaFormatId, null, [mediaFormatId], false);
            }
            else
            {
                _visualizer.Header.ApplyDetection(
                    document.PrimaryFormatId,
                    document.Metadata.ProtectionId,
                    document.FormatsDetectes.Select(format => format.FormatId),
                    document.ScpImage is not null);
            }
            _applyClassification();
            ReportProgress(DiskImageResourceKeys.ExplorerLoadingDisplay, System.IO.Path.GetFileName(path), 100, sharedProgress);
            return document;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (Exception exception)
        {
            if (!_cancellation.IsCurrentExplorer(cancellation) || cancellationToken.IsCancellationRequested) return null;
            var messageKey = DiskImageWorkspaceController.LoadFailureMessageKey(newImage, requestedFormat);
            var titleKey = messageKey == DiskImageResourceKeys.ExplorerSelectedFormatUnsupported
                ? DiskImageResourceKeys.ExplorerSelectedFormatUnsupportedTitle
                : DiskImageResourceKeys.TabExplorer;
            var presentedException = messageKey == DiskImageResourceKeys.ExplorerSelectedFormatUnsupported
                ? new DiskImageWorkspaceController.SelectedFormatUnsupportedException(
                    FormatName(requestedFormat!),
                    System.IO.Path.GetFileName(path),
                    exception)
                : exception;
            _showError(presentedException, $"Opening disk image in Explorer: {path}", titleKey, messageKey);
            return null;
        }
        finally
        {
            if (_cancellation.IsCurrentExplorer(cancellation)) _explorer.SetLoading(false);
        }
    }

    private MediaOpeningAnalysisResult? SelectCachedInterpretation(string path, bool newImage, string? requestedFormat)
    {
        if (newImage || string.IsNullOrWhiteSpace(requestedFormat) || CurrentOpeningResult is null) return null;
        if (!string.Equals(CurrentOpeningResult.DiskExploration.SourcePath, path, StringComparison.OrdinalIgnoreCase)) return null;
        var selected = CurrentOpeningResult.DiskExploration.SelectFormat(requestedFormat);
        if (selected is null) return null;
        return CurrentOpeningResult with
        {
            DiskExploration = selected
        };
    }

    private async Task<MediaOpeningAnalysisResult> AnalyzeAsync(
        string path,
        string? requestedFormat,
        CancellationToken cancellationToken,
        Action<string, string, double, bool>? sharedProgress)
    {
        var latestValue = 12d;
        var scpProgress = new Progress<ScpExplorationProgress>(item =>
        {
            var measured = item.Kind switch
            {
                ScpExplorationProgressKind.FormatProbeStarted or ScpExplorationProgressKind.FormatProbeCompleted
                    => 12d + 13d * item.Completed / Math.Max(1, item.Total),
                ScpExplorationProgressKind.RevolutionDecoded or ScpExplorationProgressKind.TrackDecoded
                    => 25d + 55d * item.Completed / Math.Max(1, item.Total),
                _ => 80d + 8d * item.Completed / Math.Max(1, item.Total)
            };
            latestValue = Math.Max(latestValue, measured);
            var detail = item.Kind switch
            {
                ScpExplorationProgressKind.FormatProbeStarted => _localize(DiskImageResourceKeys.ExplorerTestingFormat, [item.Detail]),
                ScpExplorationProgressKind.FormatProbeCompleted when item.Recognized == true => _localize(DiskImageResourceKeys.ExplorerTestedFormatRecognized, [item.Detail]),
                ScpExplorationProgressKind.FormatProbeCompleted => _localize(DiskImageResourceKeys.ExplorerTestedFormatRejected, [item.Detail]),
                ScpExplorationProgressKind.CandidateStarted => _localize(DiskImageResourceKeys.ExplorerReadingFormat, [item.Detail]),
                ScpExplorationProgressKind.CandidateCompleted when item.Recognized == true => _localize(DiskImageResourceKeys.ExplorerTestedFormatRecognized, [item.Detail]),
                ScpExplorationProgressKind.CandidateCompleted => _localize(DiskImageResourceKeys.ExplorerTestedFormatRejected, [item.Detail]),
                ScpExplorationProgressKind.RevolutionDecoded => $"{_localize(DiskImageResourceKeys.VisualAnalysingRevolution, [item.Completed, item.Total])} · {item.Detail}",
                _ => $"{_localize(DiskImageResourceKeys.VisualAnalysingTrack, [item.Completed, item.Total])} · {item.Detail}"
            };
            var stage = _localize(DiskImageResourceKeys.ExplorerLoadingRecognition, []);
            _explorer.SetLoadingProgress(stage, detail, latestValue);
            sharedProgress?.Invoke(stage, detail, latestValue, false);
        });
        return await _analyze(
            path,
            requestedFormat,
            scpProgress,
            item =>
            {
                var stage = MediaProgressText(item);
                _explorer.SetLoadingProgress(stage, item.Detail, item.Value);
                sharedProgress?.Invoke(stage, item.Detail, item.Value, false);
            },
            cancellationToken);
    }

    private void ReportProgress(
        string stageKey,
        string detail,
        double value,
        Action<string, string, double, bool>? sharedProgress)
    {
        var stage = _localize(stageKey, []);
        _explorer.SetLoadingProgress(stage, detail, value);
        sharedProgress?.Invoke(stage, detail, value, false);
    }

    private string MediaProgressText(MediaExplorationProgress progress) =>
        _localize(
            progress.Stage switch
            {
                MediaExplorationProgressStage.ReadingMedia => DiskImageResourceKeys.ExplorerLoadingMedia,
                MediaExplorationProgressStage.ReadingFileSystem when progress.MediaKind == MediaKind.Tape => DiskImageResourceKeys.ExplorerLoadingTapeDecoding,
                MediaExplorationProgressStage.ReadingFileSystem => DiskImageResourceKeys.ExplorerLoadingFileSystem,
                MediaExplorationProgressStage.RecognizingFiles when progress.MediaKind == MediaKind.Tape => DiskImageResourceKeys.ExplorerLoadingFileRecognition,
                MediaExplorationProgressStage.RecognizingFiles => DiskImageResourceKeys.ExplorerLoadingContents,
                _ => DiskImageResourceKeys.ExplorerLoadingRecognition
            },
            []);

    private string FormatName(string formatId) =>
        new BuiltInImageFormatCatalog(key => _localize(key, [])).Formats
            .FirstOrDefault(format => format.Id.Equals(formatId, StringComparison.OrdinalIgnoreCase))?.DisplayName
        ?? formatId;
}
