using GWGUI.App.Constants.DiskImages;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.Infrastructure.Processes;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.Constants;
using System.IO;

namespace GWGUI.App.Services.DiskImages.Exploration;

internal sealed class ExplorerLoadingController
{
    private readonly ExplorerSection _explorer;
    private readonly VisualizerTabSection _visualizer;
    private readonly Func<AppSettings> _getSettings;
    private readonly Func<ImageFormatDetector> _getFormatDetector;
    private readonly IGwCommandBuilder _commandBuilder;
    private readonly IGreaseweazleRunner _runner;
    private readonly Func<string, string?, CancellationToken, Task<ExploredDiskImage>> _explore;
    private readonly DiskImageExplorer _diskImageExplorer;
    private readonly bool _usesDefaultDiskImageExplorer;
    private readonly DiskImageCancellationScope _cancellation;
    private readonly Func<string, string?, CancellationToken, Action<string, string, double>?, Task<ExploredMediaImage?>> _exploreMedia;
    private readonly Action _applyClassification;
    private readonly Action<ExploredDiskImage> _rememberDiskImage;
    private readonly Action<ExploredMediaImage?> _setMediaImage;
    private readonly Action<Exception, string, string, string> _showError;
    private readonly Func<string, object[], string> _localize;

    internal ExplorerLoadingController(
        ExplorerSection explorer,
        VisualizerTabSection visualizer,
        Func<AppSettings> getSettings,
        Func<ImageFormatDetector> getFormatDetector,
        IGwCommandBuilder commandBuilder,
        IGreaseweazleRunner runner,
        Func<string, string?, CancellationToken, Task<ExploredDiskImage>> explore,
        DiskImageExplorer diskImageExplorer,
        bool usesDefaultDiskImageExplorer,
        DiskImageCancellationScope cancellation,
        Func<string, string?, CancellationToken, Action<string, string, double>?, Task<ExploredMediaImage?>> exploreMedia,
        Action applyClassification,
        Action<ExploredDiskImage> rememberDiskImage,
        Action<ExploredMediaImage?> setMediaImage,
        Action<Exception, string, string, string> showError,
        Func<string, object[], string> localize)
    {
        _explorer = explorer;
        _visualizer = visualizer;
        _getSettings = getSettings;
        _getFormatDetector = getFormatDetector;
        _commandBuilder = commandBuilder;
        _runner = runner;
        _explore = explore;
        _diskImageExplorer = diskImageExplorer;
        _usesDefaultDiskImageExplorer = usesDefaultDiskImageExplorer;
        _cancellation = cancellation;
        _exploreMedia = exploreMedia;
        _applyClassification = applyClassification;
        _rememberDiskImage = rememberDiskImage;
        _setMediaImage = setMediaImage;
        _showError = showError;
        _localize = localize;
    }

    internal string? Path { get; private set; }
    internal ExploredDiskImage? CurrentImage { get; set; }

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
            _explorer.SetLoadingProgress(_localize(DiskImageResourceKeys.ExplorerLoadingRecognition, []), System.IO.Path.GetFileName(path), 12);
            sharedProgress?.Invoke(_localize(DiskImageResourceKeys.ExplorerLoadingRecognition, []), System.IO.Path.GetFileName(path), 12, false);
            var document = SelectCachedInterpretation(path, newImage, requestedFormat);
            document ??= await ExploreWithSelectedEngineAsync(path, requestedFormat, cancellationToken, sharedProgress);
            var isScp = System.IO.Path.GetExtension(path).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase);
            var formatFoundValue = isScp ? 90d : 45d;
            _explorer.SetLoadingProgress(_localize(DiskImageResourceKeys.ExplorerLoadingFormatFound, []), document.PrimaryFormatId, formatFoundValue);
            sharedProgress?.Invoke(_localize(DiskImageResourceKeys.ExplorerLoadingFormatFound, []), document.PrimaryFormatId, formatFoundValue, false);
            if (!cancellationToken.IsCancellationRequested)
            {
                CurrentImage = document;
                _rememberDiskImage(document);
                ExploredMediaImage? mediaImage = null;
                if (isScp)
                {
                    _setMediaImage(null);
                    _explorer.Display(document);
                }
                else
                {
                    mediaImage = await _exploreMedia(path, requestedFormat, cancellationToken, (stage, detail, value) =>
                    {
                        _explorer.SetLoadingProgress(stage, detail, value);
                        sharedProgress?.Invoke(stage, detail, value, false);
                    });
                    _setMediaImage(mediaImage);
                    if (mediaImage is not null) _explorer.Display(mediaImage);
                    else _explorer.Display(document);
                }

                if (mediaImage is not null)
                {
                    var mediaFormatId = mediaImage.Document.FormatId;
                    _visualizer.Header.ApplyDetection(mediaFormatId, null, [mediaFormatId], false);
                }
                else
                {
                    var detectedFormatIds = document.FormatsDetectes.Select(format => format.FormatId).ToArray();
                    _visualizer.Header.ApplyDetection(
                        document.PrimaryFormatId,
                        document.Metadata.ProtectionId,
                        detectedFormatIds,
                        isScp);
                }
                _applyClassification();
                _explorer.SetLoadingProgress(_localize(DiskImageResourceKeys.ExplorerLoadingDisplay, []), System.IO.Path.GetFileName(path), 100);
                sharedProgress?.Invoke(_localize(DiskImageResourceKeys.ExplorerLoadingDisplay, []), System.IO.Path.GetFileName(path), 100, false);
            }
            return cancellationToken.IsCancellationRequested ? null : document;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return null; }
        catch (Exception exception)
        {
            if (!_cancellation.IsCurrentExplorer(cancellation) || cancellationToken.IsCancellationRequested) return null;
            if (_cancellation.IsCurrentExplorer(cancellation) && sharedProgress is null) _explorer.SetLoading(false);
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

    private ExploredDiskImage? SelectCachedInterpretation(string path, bool newImage, string? requestedFormat)
    {
        if (newImage || string.IsNullOrWhiteSpace(requestedFormat) || CurrentImage is null) return null;
        if (!string.Equals(CurrentImage.SourcePath, path, StringComparison.OrdinalIgnoreCase)) return null;
        return CurrentImage.SelectFormat(requestedFormat);
    }

    private async Task<ExploredDiskImage> ExploreWithSelectedEngineAsync(
        string path,
        string? requestedFormat,
        CancellationToken cancellationToken,
        Action<string, string, double, bool>? sharedProgress)
    {
        var settings = _getSettings();
        if (settings.Engines.ExplorerRead == OperationEngine.Internal)
        {
            if (_usesDefaultDiskImageExplorer
                && requestedFormat is null
                && System.IO.Path.GetExtension(path).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase))
            {
                var latestValue = 12d;
                var progress = new Progress<ScpExplorationProgress>(item =>
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
                return await _diskImageExplorer.ExploreAsync(path, null, progress, cancellationToken);
            }
            return await _explore(path, requestedFormat, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(settings.GwExecutablePath) || !File.Exists(settings.GwExecutablePath))
            throw new InvalidOperationException(_localize(DiskImageResourceKeys.AppGwNotConfigured, []));
        var detection = _getFormatDetector().Detect(path, new FileInfo(path).Length);
        var format = !string.IsNullOrWhiteSpace(requestedFormat)
            ? new BuiltInImageFormatCatalog(key => _localize(key, [])).Formats.FirstOrDefault(item => item.Id == requestedFormat)
            : detection.Format;
        if (format is null) throw new InvalidDataException(_localize(DiskImageResourceKeys.DetectionAmbiguous, []));
        var extension = format.Extensions.FirstOrDefault(item => item.IsDefault)?.Extension
            ?? format.Extensions.FirstOrDefault()?.Extension;
        if (string.IsNullOrWhiteSpace(extension) || extension.Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException(_localize(DiskImageResourceKeys.ExplorerExternalEngineUnavailable, [format.DisplayName]));

        var temporaryPath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"{DiskImageTemporaryFileConstants.ExplorerFilePrefix}{Guid.NewGuid().ToString(DiskImageTemporaryFileConstants.UniqueNameFormat)}{extension}");
        try
        {
            var output = new ConversionOutput(format.Id, extension, temporaryPath, false);
            var command = _commandBuilder.BuildConversion(settings.GwExecutablePath, path, output);
            var result = await _runner.RunAsync(command, cancellationToken: cancellationToken);
            if (!result.IsSuccess || !File.Exists(temporaryPath))
                throw new InvalidDataException(_localize(DiskImageResourceKeys.ExplorerExternalEngineFailed, [format.DisplayName]));
            return await _explore(temporaryPath, format.Id, cancellationToken);
        }
        finally
        {
            TryDelete(temporaryPath);
        }
    }

    private string FormatName(string formatId) =>
        new BuiltInImageFormatCatalog(key => _localize(key, [])).Formats
            .FirstOrDefault(format => format.Id.Equals(formatId, StringComparison.OrdinalIgnoreCase))?.DisplayName
        ?? formatId;

    private static void TryDelete(string? path)
    {
        try { if (path is not null && File.Exists(path)) File.Delete(path); }
        catch { }
    }
}
