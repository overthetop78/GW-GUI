using GWGUI.Infrastructure.Commands.Building;
using GWGUI.Infrastructure.Commands.Execution;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Images.Formats.Detection;
using GWGUI.Infrastructure.Settings;
using GWGUI.App.Contracts.Progress;
using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Contracts.Visualization;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Functions.Services.Visualization;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Services.Visualization;
using GWGUI.App.Services.DiskImages.Visualization;
using GWGUI.App.Services.DiskImages.Exploration;
using GWGUI.App.Services.DiskImages.Selection;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Views.Controls.Visualization;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Models.Flux;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Images.Visualization;
using GWGUI.MediaEngine.Exploration.Contracts;
using GWGUI.MediaEngine.Exploration.Results;
using System.IO;
using System.Windows.Threading;

namespace GWGUI.App.Services.DiskImages;

internal sealed class DiskImageWorkspaceController : IDisposable
{
    private readonly ExplorerSection _explorer;
    private readonly VisualizerTabSection _visualizer;
    private readonly Func<string, string?, CancellationToken, Task<ExploredDiskImage>> _explore;
    private readonly MediaVisualizationProviderRegistry? _visualizationProviders;
    private readonly MediaVisualizationController _mediaVisualization;
    private readonly ScpVisualizationController _scpVisualization;
    private readonly ExplorerPresentationController _explorerLoading;
    private readonly MediaImageExplorationService? _mediaExploration;
    private readonly MediaOpeningAnalysisService? _mediaOpeningAnalysis;
    private readonly CassetteLoadingPresenter _cassetteLoading;
    private readonly DiskImageFileSelectionService _fileSelection;
    private readonly VisualizerLoadingController _visualizerLoading;
    private readonly Action<Exception, string, string, string> _showError;
    private readonly Func<string, object[], string> _localize;
    private readonly DiskImageCancellationScope _cancellation;
    private readonly object _sharedLoadSync = new();
    private ExploredDiskImage? _visualizerExploredImage;
    private ExploredMediaImage? _exploredMediaImage;
    private SharedLoadRequest? _pendingSharedLoad;
    private bool _sharedLoadPumpRunning;
    internal IReadOnlyList<(int Head, int Cylinder)> ScpTrackPresentationOrder =>
        _scpVisualization.TrackPresentationOrder;
    internal int SectorRevealedTrackCount => _mediaVisualization.SectorRevealedTrackCount;

    public DiskImageWorkspaceController(
        ExplorerSection explorer,
        VisualizerTabSection visualizer,
        MainWindowViewModel viewModel,
        TrackProgressStrip face0Progress,
        TrackProgressStrip face1Progress,
        Func<AppSettings> getSettings,
        Func<ImageFormatDetector> getFormatDetector,
        Func<GwFormatCapabilities> getCapabilities,
        IFileDialogService fileDialogs,
        IGwCommandBuilder commandBuilder,
        IGreaseweazleRunner visualizationRunner,
        ScpInspectorController inspector,
        ScpDocumentLoader scpLoader,
        DiskImageExplorer diskImageExplorer,
        DiskImageCancellationScope cancellation,
        Func<bool> operationIsRunning,
        Action<Exception, string, string, string> showError,
        Func<string, object[], string> localize,
        Func<string, string?, CancellationToken, Task<ExploredDiskImage>>? explore = null,
        MediaImageReadingService? mediaReader = null,
        MediaExplorer? mediaExplorer = null,
        MediaVisualizationProviderRegistry? visualizationProviders = null)
    {
        _explorer = explorer;
        _visualizer = visualizer;
        _explore = explore ?? diskImageExplorer.ExploreAsync;
        _visualizationProviders = visualizationProviders;
        if (mediaReader is not null && mediaExplorer is not null)
        {
            _mediaExploration = new MediaImageExplorationService(mediaReader, mediaExplorer);
            _mediaOpeningAnalysis = new MediaOpeningAnalysisService(
                mediaReader,
                diskImageExplorer,
                mediaExplorer);
        }
        _fileSelection = new DiskImageFileSelectionService(getSettings, fileDialogs, localize);
        _mediaVisualization = new MediaVisualizationController(
            visualizer,
            viewModel,
            face0Progress,
            face1Progress,
            visualizationProviders,
            localize);
        _scpVisualization = new ScpVisualizationController(
            visualizer,
            viewModel,
            face0Progress,
            face1Progress,
            inspector,
            scpLoader,
            cancellation,
            visualizationProviders,
            operationIsRunning,
            showError,
            localize);
        _explorerLoading = new ExplorerPresentationController(
            explorer,
            visualizer,
            AnalyzeOpeningAsync,
            cancellation,
            ApplyClassification,
            document => LastReadImage = document,
            mediaImage => _exploredMediaImage = mediaImage,
            showError,
            localize);
        _cassetteLoading = new CassetteLoadingPresenter(
            explorer,
            visualizer,
            viewModel,
            face0Progress,
            face1Progress,
            _mediaVisualization,
            ReportSharedProgress,
            localize);
        _visualizerLoading = new VisualizerLoadingController(
            visualizer,
            getSettings,
            getFormatDetector,
            getCapabilities,
            commandBuilder,
            visualizationRunner,
            cancellation,
            _mediaExploration,
            _mediaVisualization,
            _scpVisualization,
            () => _exploredMediaImage,
            mediaImage => _exploredMediaImage = mediaImage,
            (path, token) => AnalyzeAsync(path, token),
            RememberReadImage,
            ApplyScpDetection,
            ClearVisualizer,
            operationIsRunning);
        _cancellation = cancellation;
        _showError = showError;
        _localize = localize;
    }

    private async Task<MediaOpeningAnalysisResult> AnalyzeOpeningAsync(
        string path,
        string? requestedFormatId,
        IProgress<GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection.ScpExplorationProgress>? scpProgress,
        Action<MediaExplorationProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (_mediaOpeningAnalysis is not null)
        {
            return await _mediaOpeningAnalysis.AnalyzeAsync(
                path,
                requestedFormatId,
                scpProgress,
                progress,
                cancellationToken);
        }

        var disk = await _explore(path, requestedFormatId, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var representation = disk.ScpImage is { } scpImage
            ? (GWGUI.MediaEngine.Interfaces.IMediaImageRepresentation)new FluxMediaImageRepresentation(
                ScpProtectedTrackImageAdapter.Create(scpImage))
            : new SectorMediaImageRepresentation(disk.Image);
        var document = MediaImageDocument.CreateUnexplored(
            new MediaSourceDescriptor(path, [], RequestedFormatId: requestedFormatId),
            disk.PrimaryFormatId,
            MediaKind.Floppy,
            representation,
            [],
            new Dictionary<string, string>(StringComparer.Ordinal));
        return new(document, disk, null);
    }

    public string? ExplorerPath => _explorerLoading.Path;
    public string? LastCapturedPath { get; set; }
    public IImageDisquette? LastReadImage { get; private set; }

    public async Task<ExploredDiskImage> AnalyzeAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var openingResult = _mediaOpeningAnalysis is null
            ? null
            : await _mediaOpeningAnalysis.AnalyzeAsync(path, cancellationToken: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var document = openingResult?.DiskExploration ?? await _explore(path, null, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        _exploredMediaImage = openingResult?.MediaExploration;
        _visualizer.Header.ApplyDetection(
            document.PrimaryFormatId,
            document.Metadata.ProtectionId,
            document.FormatsDetectes.Select(format => format.FormatId),
            openingResult?.Document.Representation.RepresentationKind == MediaRepresentationKind.Flux
                || document.ScpImage is not null);
        LastReadImage = document;
        return document;
    }

    public void RememberReadImage(IImageDisquette image)
    {
        ArgumentNullException.ThrowIfNull(image);
        LastReadImage = image;
    }

    public string? SelectVisualizerImage() => _fileSelection.SelectVisualizerImage();

    public string? SelectExplorerImage() => _fileSelection.SelectExplorerImage();

    public Task LoadAsync(string path, string? displayFileName = null)
    {
        var request = new SharedLoadRequest(
            path,
            displayFileName,
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously));
        var startPump = false;
        lock (_sharedLoadSync)
        {
            _pendingSharedLoad?.Completion.TrySetResult(false);
            _pendingSharedLoad = request;
            if (!_sharedLoadPumpRunning)
            {
                _sharedLoadPumpRunning = true;
                startPump = true;
            }
        }

        _cancellation.CancelAll();
        if (startPump) _ = ProcessSharedLoadsAsync();
        return request.Completion.Task;
    }

    private async Task ProcessSharedLoadsAsync()
    {
        while (true)
        {
            SharedLoadRequest request;
            lock (_sharedLoadSync)
            {
                if (_pendingSharedLoad is null)
                {
                    _sharedLoadPumpRunning = false;
                    return;
                }

                request = _pendingSharedLoad;
                _pendingSharedLoad = null;
            }

            var cancellation = _cancellation.BeginSharedLoad();
            var completed = true;
            Exception? failure = null;
            try
            {
                await LoadCoreAsync(request.Path, request.DisplayFileName, cancellation.Token);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                completed = false;
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                _cancellation.CompleteSharedLoad(cancellation);
            }

            if (failure is not null) request.Completion.TrySetException(failure);
            else request.Completion.TrySetResult(completed);
        }
    }

    private async Task LoadCoreAsync(
        string path,
        string? displayFileName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var shownName = displayFileName ?? Path.GetFileName(path);
        ClearVisualizer(shownName);
        ReportSharedProgress(_localize(DiskImageResourceKeys.ExplorerLoadingRecognition, []), shownName, 0, true);
        await _visualizer.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var explored = await LoadExplorerAsync(path, true, ReportSharedProgress);
            cancellationToken.ThrowIfCancellationRequested();
            if (explored is null) return;
            var openingResult = _explorerLoading.CurrentOpeningResult;
            if (openingResult is null || !ReferenceEquals(openingResult.DiskExploration, explored)) return;
            await _visualizerLoading.LoadAsync(path, displayFileName, explored, openingResult);
            cancellationToken.ThrowIfCancellationRequested();
        }
        finally
        {
            try
            {
                if (!cancellationToken.IsCancellationRequested
                    && _explorerLoading.CurrentOpeningResult?.Document.MediaKind == MediaKind.Tape)
                {
                    await _cassetteLoading.CompleteAsync(
                        cancellationToken,
                        shownName,
                        _exploredMediaImage);
                }
            }
            finally
            {
                _explorer.SetLoading(false);
                _scpVisualization.HideProgress();
            }
        }
    }

    public Task<ExploredDiskImage?> LoadExplorerAsync(string path, bool newImage = true) =>
        _explorerLoading.LoadAsync(path, newImage);

    private async Task<ExploredDiskImage?> LoadExplorerAsync(
        string path,
        bool newImage,
        Action<string, string, double, bool>? sharedProgress) =>
        await _explorerLoading.LoadAsync(path, newImage, sharedProgress);

    internal static string LoadFailureMessageKey(bool newImage, string? requestedFormat) =>
        !newImage && !string.IsNullOrWhiteSpace(requestedFormat)
            ? DiskImageResourceKeys.ExplorerSelectedFormatUnsupported
            : DiskImageResourceKeys.ExplorerLoadFailed;

    private string FormatName(string formatId) =>
        new BuiltInImageFormatCatalog(key => _localize(key, [])).Formats
            .FirstOrDefault(format => format.Id.Equals(formatId, StringComparison.OrdinalIgnoreCase))?.DisplayName
        ?? formatId;

    internal sealed class SelectedFormatUnsupportedException(
        string formatName,
        string fileName,
        Exception innerException) : Exception(innerException.Message, innerException)
    {
        internal string FormatName { get; } = formatName;
        internal string FileName { get; } = fileName;
    }

    public Task LoadVisualizerAsync(
        string path,
        string? displayFileName = null,
        ExploredDiskImage? exploredImage = null) =>
        _visualizerLoading.LoadAsync(path, displayFileName, exploredImage);

    public Task LoadScpAsync(string path, string? displayFileName = null) =>
        _scpVisualization.LoadAsync(path, displayFileName);

    private void ApplyScpDetection(ExploredDiskImage detected)
    {
        _visualizerExploredImage = detected;
        LastReadImage = detected;
        var detectedFormatIds = detected.FormatsDetectes.Select(format => format.FormatId).ToArray();
        _visualizer.Header.ApplyDetection(
            detected.PrimaryFormatId,
            detected.Metadata.ProtectionId,
            detectedFormatIds);
        ApplyClassification();
    }

    public void ApplyClassification()
    {
        var decoder = _visualizer.Header.DecoderCombo;
        if (decoder.ItemsSource is null) return;
        var selector = _visualizer.Header.ClassificationSelector;
        var classification = DiskVisualizationClassificationFunctions.Resolve(
            selector.SelectedMachine,
            selector.SelectedFormatId,
            selector.SelectedProtectionId,
            selector.AutomaticDetection);
        var choice = decoder.Items.Cast<ScpDecoderChoice>()
            .FirstOrDefault(item => string.Equals(item.Id, classification.DecoderId, StringComparison.OrdinalIgnoreCase));
        if (choice is not null && !Equals(decoder.SelectedItem, choice)) decoder.SelectedItem = choice;
        _visualizer.FirstSide.SetMediaCategory(classification.MediaCategory);
        _visualizer.SecondSide.SetMediaCategory(classification.MediaCategory);
    }

    public async Task SelectVisualizerRepresentationAsync(string? formatId)
    {
        try
        {
            _visualizer.Header.SelectRepresentation(formatId);
            await SelectVisualizerRepresentationCoreAsync(formatId);
            if (!string.IsNullOrWhiteSpace(formatId)
                && SameExplorerAndVisualizerSource()
                && _explorerLoading.CurrentImage?.SelectFormat(formatId) is { } explorerSelection)
            {
                _explorerLoading.CurrentImage = explorerSelection;
                _explorer.Display(explorerSelection);
            }
        }
        catch (OperationCanceledException)
        {
            // Un changement de représentation annule normalement la présentation précédente.
        }
        catch (Exception exception)
        {
            if (string.IsNullOrWhiteSpace(formatId)) return;
            var sourcePath = VisualizerSourcePath();
            var presentedException = new SelectedFormatUnsupportedException(
                FormatName(formatId),
                Path.GetFileName(sourcePath ?? string.Empty),
                exception);
            _showError(
                presentedException,
                $"Selecting Visualizer format {formatId} for disk image: {sourcePath}",
                "Explorer.SelectedFormatUnsupportedTitle",
                "Explorer.SelectedFormatUnsupported");
        }
    }

    public async Task SelectExplorerRepresentationAsync()
    {
        var formatId = _explorer.SelectedFormatId;
        if (string.IsNullOrWhiteSpace(formatId) || string.IsNullOrWhiteSpace(ExplorerPath))
        {
            return;
        }

        var explorerSelection = _explorerLoading.CurrentImage?.SelectFormat(formatId);
        if (explorerSelection is null)
        {
            var explicitlyExplored = await LoadExplorerAsync(ExplorerPath, false);
            explorerSelection = explicitlyExplored?.SelectFormat(formatId);
            if (explorerSelection is null) return;
        }

        _explorerLoading.CurrentImage = explorerSelection;
        _explorer.Display(explorerSelection);
        if (!SameExplorerAndVisualizerSource())
        {
            return;
        }

        _visualizerExploredImage = explorerSelection;
        _visualizer.Header.SelectRepresentation(formatId);
        await SelectVisualizerRepresentationAsync(formatId);
    }

    private bool SameExplorerAndVisualizerSource() =>
        !string.IsNullOrWhiteSpace(ExplorerPath)
        && !string.IsNullOrWhiteSpace(_scpVisualization.SourcePath)
        && string.Equals(ExplorerPath, _scpVisualization.SourcePath, StringComparison.OrdinalIgnoreCase);

    private async Task SelectVisualizerRepresentationCoreAsync(string? formatId)
    {
        var sourcePath = VisualizerSourcePath();
        if (string.IsNullOrWhiteSpace(sourcePath)) return;
        var cancellation = _cancellation.BeginVisualization();
        var cancellationToken = cancellation.Token;
        if (string.IsNullOrWhiteSpace(formatId))
        {
            await _scpVisualization.RestoreFluxRepresentationAsync(cancellationToken);
            if (_cancellation.IsCurrentVisualization(cancellation)) _scpVisualization.HideProgress();
            return;
        }

        var selected = _visualizerExploredImage?.SelectFormat(formatId)
            ?? _explorerLoading.CurrentImage?.SelectFormat(formatId)
            ?? await _explore(sourcePath, formatId, cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
        _visualizerExploredImage = selected;
        var document = MediaImageDocument.CreateUnexplored(
            new MediaSourceDescriptor(sourcePath, [], RequestedFormatId: formatId),
            selected.Image.FormatId,
            MediaKind.Floppy,
            new SectorMediaImageRepresentation(selected.Image),
            [],
            new Dictionary<string, string>(StringComparer.Ordinal));
        var descriptor = _visualizationProviders?.CreateDescriptor(document);
        if (descriptor is null) return;
        await _mediaVisualization.PresentSectorImageAsync(
            document,
            descriptor,
            selected.Image,
            cancellationToken);
    }

    private string? VisualizerSourcePath() =>
        _scpVisualization.SourcePath
        ?? _visualizerExploredImage?.SourcePath
        ?? _explorerLoading.CurrentImage?.SourcePath
        ?? _exploredMediaImage?.Document.Source.PrimaryPath
        ?? ExplorerPath;

    public void ClearVisualizer(string fileName)
    {
        _visualizerExploredImage = null;
        _scpVisualization.Clear(fileName);
    }

    public void CancelAll() => _cancellation.CancelAll();

    public Task PrepareViewsForInspectorAsync(CancellationToken cancellationToken) =>
        _scpVisualization.PrepareViewsAsync(cancellationToken);

    public void HideProgressForInspector() => _scpVisualization.HideProgress();

    public void Dispose() => _cancellation.Dispose();

    internal void ReportSharedProgress(string stage, string detail, double value, bool indeterminate)
        => _scpVisualization.ReportSharedProgress(stage, detail, value, indeterminate);

    internal static TimeSpan RemainingSectorTrackPresentationDelay(TimeSpan elapsedSincePresentation)
        => MediaVisualizationController.RemainingSectorTrackPresentationDelay(elapsedSincePresentation);

    private sealed record SharedLoadRequest(
        string Path,
        string? DisplayFileName,
        TaskCompletionSource<bool> Completion);

}
