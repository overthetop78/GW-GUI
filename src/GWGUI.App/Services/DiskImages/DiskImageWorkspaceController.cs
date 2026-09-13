using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.App.Contracts.Progress;
using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Contracts.Rendering.Sectors;
using GWGUI.App.Contracts.Visualization;
using GWGUI.App.Functions.Services.Visualization;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Visualization;
using GWGUI.App.Presenters.Visualization;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaEngine.Representations.Flux;
using GWGUI.MediaEngine.Visualization;
using System.IO;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using GWGUI.Infrastructure.Processes;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Contracts;
using GWGUI.MediaEngine.Exploration.Scp;


using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.App.Services.DiskImages;

internal sealed class DiskImageWorkspaceController : IDisposable
{
    private static readonly TimeSpan MinimumTrackPresentationInterval = TimeSpan.FromMilliseconds(30);
    private static readonly TimeSpan MinimumSectorTrackPresentationInterval = TimeSpan.FromMilliseconds(100);
    private readonly ExplorerSection _explorer;
    private readonly VisualizerTabSection _visualizer;
    private readonly MainWindowViewModel _viewModel;
    private readonly TrackProgressStrip _face0Progress;
    private readonly TrackProgressStrip _face1Progress;
    private readonly Func<AppSettings> _getSettings;
    private readonly Func<ImageFormatDetector> _getFormatDetector;
    private readonly Func<GwFormatCapabilities> _getCapabilities;
    private readonly IFileDialogService _fileDialogs;
    private readonly IGwCommandBuilder _commandBuilder;
    private readonly IGreaseweazleRunner _visualizationRunner;
    private readonly ScpInspectorController _inspector;
    private readonly ScpDocumentLoader _scpLoader;
    private readonly Func<string, string?, CancellationToken, Task<ExploredDiskImage>> _explore;
    private readonly DiskImageExplorer _diskImageExplorer;
    private readonly bool _usesDefaultDiskImageExplorer;
    private readonly MediaImageReadingService? _mediaReader;
    private readonly MediaExplorer? _mediaExplorer;
    private readonly MediaVisualizationProviderRegistry? _visualizationProviders;
    private readonly SectorMediaView _sectorView;
    private readonly SectorMediaInspectorPresenter _sectorPresenter;
    private readonly BlockMediaView _blockView;
    private readonly BlockMediaInspectorPresenter _blockPresenter;
    private readonly OpticalMediaView _opticalView;
    private readonly OpticalMediaInspectorPresenter _opticalPresenter;
    private readonly SequentialMediaView _sequentialView;
    private readonly SequentialMediaInspectorPresenter _sequentialPresenter;
    private readonly Func<bool> _operationIsRunning;
    private readonly Action<Exception, string, string, string> _showError;
    private readonly Func<string, object[], string> _localize;
    private readonly DiskImageCancellationScope _cancellation;
    private ScpImage? _scpImage;
    private string? _scpPath;
    private string? _scpDisplayName;
    private string? _scpSummary;
    private readonly Dictionary<(int Head, int Cylinder), ScpTrackPreparation> _scpTrackPreparations = [];
    private readonly List<(int Head, int Cylinder)> _scpTrackPresentationOrder = [];
    private ExploredDiskImage? _explorerExploredImage;
    private ExploredDiskImage? _visualizerExploredImage;
    private ExploredMediaImage? _exploredMediaImage;
    private MediaVisualizationDescriptor? _visualizationDescriptor;
    private GWGUI.App.Contracts.Rendering.Blocks.BlockMediaRenderModel? _blockRenderModel;
    private GWGUI.App.Contracts.Rendering.Blocks.BlockMediaRange? _selectedBlockRange;
    private int? _selectedBlockSurface;
    private MediaImageDocument? _opticalDocument;
    private GWGUI.App.Contracts.Rendering.Optical.OpticalMediaRenderModel? _opticalRenderModel;
    private GWGUI.App.Contracts.Rendering.Sequential.SequentialMediaRenderModel? _sequentialRenderModel;
    private long _sharedLoadGeneration;
    internal IReadOnlyList<(int Head, int Cylinder)> ScpTrackPresentationOrder => _scpTrackPresentationOrder;
    internal int SectorRevealedTrackCount => _sectorView.RevealedTrackCount;

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
        _viewModel = viewModel;
        _face0Progress = face0Progress;
        _face1Progress = face1Progress;
        _getSettings = getSettings;
        _getFormatDetector = getFormatDetector;
        _getCapabilities = getCapabilities;
        _fileDialogs = fileDialogs;
        _commandBuilder = commandBuilder;
        _visualizationRunner = visualizationRunner;
        _inspector = inspector;
        _scpLoader = scpLoader;
        _diskImageExplorer = diskImageExplorer;
        _usesDefaultDiskImageExplorer = explore is null;
        _explore = explore ?? diskImageExplorer.ExploreAsync;
        _mediaReader = mediaReader;
        _mediaExplorer = mediaExplorer;
        _visualizationProviders = visualizationProviders;
        _sectorView = new SectorMediaView();
        _sectorPresenter = new SectorMediaInspectorPresenter(localize);
        _sectorView.SectorSelected += HandleSectorSelected;
        _visualizer.RegisterRepresentationView(MediaRepresentationKind.Sectors, _sectorView);
        _blockView = new BlockMediaView();
        _blockPresenter = new BlockMediaInspectorPresenter(localize);
        _blockView.RangeSelected += HandleBlockRangeSelected;
        _blockView.SurfaceSelected += HandleBlockSurfaceSelected;
        _visualizer.RegisterRepresentationView(MediaRepresentationKind.Blocks, _blockView);
        _opticalView = new OpticalMediaView();
        _opticalPresenter = new OpticalMediaInspectorPresenter(localize);
        _opticalView.TrackSelected += HandleOpticalTrackSelected;
        _visualizer.RegisterRepresentationView(MediaRepresentationKind.OpticalTracks, _opticalView);
        _sequentialView = new SequentialMediaView();
        _sequentialPresenter = new SequentialMediaInspectorPresenter(localize);
        _sequentialView.SegmentSelected += HandleSequentialSegmentSelected;
        _visualizer.RegisterRepresentationView(MediaRepresentationKind.Sequential, _sequentialView);
        _cancellation = cancellation;
        _operationIsRunning = operationIsRunning;
        _showError = showError;
        _localize = localize;
    }

    public string? ExplorerPath { get; private set; }
    public string? LastCapturedPath { get; set; }
    public IImageDisquette? LastReadImage { get; private set; }

    public async Task<ExploredDiskImage> AnalyzeAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var document = await _explore(path, null, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        _exploredMediaImage = await ExploreMediaAsync(path, null, cancellationToken);
        _visualizer.Header.ApplyDetection(
            document.PrimaryFormatId,
            document.Metadata.ProtectionId,
            document.FormatsDetectes.Select(format => format.FormatId),
            Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase));
        LastReadImage = document;
        return document;
    }

    public void RememberReadImage(IImageDisquette image)
    {
        ArgumentNullException.ThrowIfNull(image);
        LastReadImage = image;
    }

    public string? SelectVisualizerImage() => SelectImage(
        settings => settings.LastVisualizerImageFolder,
        (settings, folder) => settings.LastVisualizerImageFolder = folder);

    public string? SelectExplorerImage() => SelectImage(
        settings => settings.LastExplorerImageFolder,
        (settings, folder) => settings.LastExplorerImageFolder = folder);

    private string? SelectImage(
        Func<AppSettings, string?> getLastFolder,
        Action<AppSettings, string?> setLastFolder)
    {
        var settings = _getSettings();
        var lastFolder = getLastFolder(settings);
        var initialDirectory = !string.IsNullOrWhiteSpace(lastFolder) && Directory.Exists(lastFolder)
            ? lastFolder
            : settings.DefaultImagesFolder;
        var path = _fileDialogs.OpenFile(new(_localize("Common.DiskImageFilter", []), initialDirectory));
        if (path is not null) setLastFolder(settings, Path.GetDirectoryName(path));
        return path;
    }

    public async Task LoadAsync(string path, string? displayFileName = null)
    {
        var generation = Interlocked.Increment(ref _sharedLoadGeneration);
        var shownName = displayFileName ?? Path.GetFileName(path);
        ClearVisualizer(shownName);
        ReportSharedProgress(_localize("Explorer.LoadingRecognition", []), shownName, 0, true);
        try
        {
            if (Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase))
            {
                var visualizerTask = LoadScpCoreAsync(path, displayFileName, preserveProgress: true);
                var explorerTask = LoadExplorerAsync(path, true, ReportSharedProgress);
                await visualizerTask;
                var explored = await explorerTask;
                if (explored is null || generation != Volatile.Read(ref _sharedLoadGeneration)) return;
                ApplyScpDetection(explored);
                return;
            }

            var nonScpExplored = await LoadExplorerAsync(path, true, ReportSharedProgress);
            if (nonScpExplored is null || generation != Volatile.Read(ref _sharedLoadGeneration)) return;
            await LoadVisualizerAsync(path, displayFileName, nonScpExplored);
        }
        finally
        {
            if (generation == Volatile.Read(ref _sharedLoadGeneration)) HideProgress();
        }
    }

    public Task<ExploredDiskImage?> LoadExplorerAsync(string path, bool newImage = true) =>
        LoadExplorerAsync(path, newImage, null);

    private async Task<ExploredDiskImage?> LoadExplorerAsync(
        string path,
        bool newImage,
        Action<string, string, double, bool>? sharedProgress)
    {
        var cancellation = _cancellation.BeginExplorer();
        var cancellationToken = cancellation.Token;
        ExplorerPath = path;
        _explorer.Clear(path, newImage);
        _explorer.SetLoading(true);
        var requestedFormat = newImage ? _explorer.FormatIdForNewImage : _explorer.SelectedFormatId;
        try
        {
            _explorer.SetLoadingProgress(
                _localize("Explorer.LoadingRecognition", []),
                Path.GetFileName(path),
                12);
            sharedProgress?.Invoke(
                _localize("Explorer.LoadingRecognition", []),
                Path.GetFileName(path),
                12,
                false);
            var document = SelectCachedInterpretation(path, newImage, requestedFormat);
            document ??= await ExploreWithSelectedEngineAsync(path, requestedFormat, cancellationToken, sharedProgress);
            var isScp = Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase);
            var formatFoundValue = isScp ? 90d : 45d;
            _explorer.SetLoadingProgress(
                _localize("Explorer.LoadingFormatFound", []),
                document.PrimaryFormatId,
                formatFoundValue);
            sharedProgress?.Invoke(
                _localize("Explorer.LoadingFormatFound", []),
                document.PrimaryFormatId,
                formatFoundValue,
                false);
            if (!cancellationToken.IsCancellationRequested)
            {
                _explorerExploredImage = document;
                LastReadImage = document;
                if (isScp)
                {
                    _exploredMediaImage = null;
                    _explorer.Display(document);
                }
                else
                {
                    _exploredMediaImage = await ExploreMediaAsync(
                        path,
                        requestedFormat,
                        cancellationToken,
                        (stage, detail, value) =>
                        {
                            _explorer.SetLoadingProgress(stage, detail, value);
                            sharedProgress?.Invoke(stage, detail, value, false);
                        });
                    if (_exploredMediaImage?.Document.Representation is SectorMediaImageRepresentation)
                        _explorer.Display(_exploredMediaImage);
                    else
                        _explorer.Display(document);
                }
                var detectedFormatIds = document.FormatsDetectes
                    .Select(format => format.FormatId)
                    .ToArray();
                _visualizer.Header.ApplyDetection(
                    document.PrimaryFormatId,
                    document.Metadata.ProtectionId,
                    detectedFormatIds,
                    Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase));
                ApplyClassification();
                _explorer.SetLoadingProgress(
                    _localize("Explorer.LoadingDisplay", []),
                    Path.GetFileName(path),
                    100);
                sharedProgress?.Invoke(
                    _localize("Explorer.LoadingDisplay", []),
                    Path.GetFileName(path),
                    100,
                    false);
            }
            return cancellationToken.IsCancellationRequested ? null : document;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return null; }
        catch (Exception exception)
        {
            if (!_cancellation.IsCurrentExplorer(cancellation) || cancellationToken.IsCancellationRequested) return null;
            if (_cancellation.IsCurrentExplorer(cancellation)) _explorer.SetLoading(false);
            var messageKey = LoadFailureMessageKey(newImage, requestedFormat);
            var titleKey = messageKey == "Explorer.SelectedFormatUnsupported"
                ? "Explorer.SelectedFormatUnsupportedTitle"
                : "Tab.Explorer";
            var presentedException = messageKey == "Explorer.SelectedFormatUnsupported"
                ? new SelectedFormatUnsupportedException(FormatName(requestedFormat!), Path.GetFileName(path), exception)
                : exception;
            _showError(presentedException, $"Opening disk image in Explorer: {path}", titleKey, messageKey);
            return null;
        }
        finally
        {
            if (_cancellation.IsCurrentExplorer(cancellation)) _explorer.SetLoading(false);
        }
    }

    internal static string LoadFailureMessageKey(bool newImage, string? requestedFormat) =>
        !newImage && !string.IsNullOrWhiteSpace(requestedFormat)
            ? "Explorer.SelectedFormatUnsupported"
            : "Explorer.LoadFailed";

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

    private ExploredDiskImage? SelectCachedInterpretation(
        string path,
        bool newImage,
        string? requestedFormat)
    {
        if (newImage || string.IsNullOrWhiteSpace(requestedFormat) || _explorerExploredImage is null)
        {
            return null;
        }

        if (!string.Equals(_explorerExploredImage.SourcePath, path, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return _explorerExploredImage.SelectFormat(requestedFormat);
    }

    private async Task<ExploredDiskImage> ExploreWithSelectedEngineAsync(
        string path,
        string? requestedFormat,
        CancellationToken cancellationToken,
        Action<string, string, double, bool>? sharedProgress = null)
    {
        var settings = _getSettings();
        if (settings.Engines.ExplorerRead == OperationEngine.Internal)
        {
            if (_usesDefaultDiskImageExplorer
                && requestedFormat is null
                && Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase))
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
                        ScpExplorationProgressKind.FormatProbeStarted
                            => _localize("Explorer.TestingFormat", [item.Detail]),
                        ScpExplorationProgressKind.FormatProbeCompleted when item.Recognized == true
                            => _localize("Explorer.TestedFormatRecognized", [item.Detail]),
                        ScpExplorationProgressKind.FormatProbeCompleted
                            => _localize("Explorer.TestedFormatRejected", [item.Detail]),
                        ScpExplorationProgressKind.CandidateStarted
                            => _localize("Explorer.ReadingFormat", [item.Detail]),
                        ScpExplorationProgressKind.CandidateCompleted when item.Recognized == true
                            => _localize("Explorer.TestedFormatRecognized", [item.Detail]),
                        ScpExplorationProgressKind.CandidateCompleted
                            => _localize("Explorer.TestedFormatRejected", [item.Detail]),
                        ScpExplorationProgressKind.RevolutionDecoded
                            => $"{_localize("Visual.AnalysingRevolution", [item.Completed, item.Total])} · {item.Detail}",
                        _ => $"{_localize("Visual.AnalysingTrack", [item.Completed, item.Total])} · {item.Detail}"
                    };
                    var stage = _localize("Explorer.LoadingRecognition", []);
                    _explorer.SetLoadingProgress(stage, detail, latestValue);
                    sharedProgress?.Invoke(stage, detail, latestValue, false);
                });
                return await _diskImageExplorer.ExploreAsync(path, null, progress, cancellationToken);
            }
            return await _explore(path, requestedFormat, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(settings.GwExecutablePath) || !File.Exists(settings.GwExecutablePath))
            throw new InvalidOperationException(_localize("App.GwNotConfigured", []));

        var detection = _getFormatDetector().Detect(path, new FileInfo(path).Length);
        var format = !string.IsNullOrWhiteSpace(requestedFormat)
            ? new BuiltInImageFormatCatalog(key => _localize(key, [])).Formats.FirstOrDefault(item => item.Id == requestedFormat)
            : detection.Format;
        if (format is null)
            throw new InvalidDataException(_localize("Detection.Ambiguous", []));

        var extension = format.Extensions.FirstOrDefault(item => item.IsDefault)?.Extension
            ?? format.Extensions.FirstOrDefault()?.Extension;
        if (string.IsNullOrWhiteSpace(extension) || extension.Equals(".scp", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException(_localize("Explorer.ExternalEngineUnavailable", [format.DisplayName]));

        var temporaryPath = Path.Combine(Path.GetTempPath(), $"gwgui-explorer-{Guid.NewGuid():N}{extension}");
        try
        {
            var output = new ConversionOutput(format.Id, extension, temporaryPath, false);
            var command = _commandBuilder.BuildConversion(settings.GwExecutablePath, path, output);
            var result = await _visualizationRunner.RunAsync(command, cancellationToken: cancellationToken);
            if (!result.IsSuccess || !File.Exists(temporaryPath))
                throw new InvalidDataException(_localize("Explorer.ExternalEngineFailed", [format.DisplayName]));
            return await _explore(temporaryPath, format.Id, cancellationToken);
        }
        finally
        {
            TryDelete(temporaryPath);
        }
    }

    public async Task LoadVisualizerAsync(string path, string? displayFileName = null, ExploredDiskImage? exploredImage = null)
    {
        var cancellation = _cancellation.BeginVisualization();
        var cancellationToken = cancellation.Token;
        if (Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            _visualizer.Header.ApplyDetection(null, null, [], true);
            var detectionTask = exploredImage is null
                ? _explore(path, null, cancellationToken)
                : Task.FromResult(exploredImage);
            try
            {
                await LoadScpAsync(path, displayFileName);
                cancellationToken.ThrowIfCancellationRequested();
                var detected = await detectionTask;
                cancellationToken.ThrowIfCancellationRequested();
                ApplyScpDetection(detected);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception exception)
            {
                ErrorLog.Write(exception, $"Detecting formats in SCP image: {path}");
            }
            return;
        }

        _scpImage = null;
        _scpPath = null;
        _scpDisplayName = null;
        _scpSummary = null;
        _scpTrackPreparations.Clear();
        _scpTrackPresentationOrder.Clear();
        _inspector.ClearImage();
        _visualizer.Header.ClearRevolutions();

        var explored = exploredImage;
        try
        {
            if (explored is null)
            {
                explored = await AnalyzeAsync(path, cancellationToken);
            }
            else
            {
                LastReadImage = explored;
                if (!string.Equals(
                        _exploredMediaImage?.Document.Source.PrimaryPath,
                        path,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _exploredMediaImage = await ExploreMediaAsync(path, explored.PrimaryFormatId, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        {
            explored = null;
        }

        if (cancellationToken.IsCancellationRequested) return;

        bool presented;
        try
        {
            presented = _exploredMediaImage is not null
                && await PresentMediaVisualizationAsync(_exploredMediaImage.Document, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (presented)
        {
            _scpImage = null;
            _inspector.ClearImage();
            HideProgress();
            return;
        }

        ClearVisualizer(displayFileName ?? Path.GetFileName(path));
        if (_operationIsRunning()) return;
        cancellationToken.ThrowIfCancellationRequested();

        var detection = _getFormatDetector().Detect(path, new FileInfo(path).Length);
        if (!GwVisualizationPolicy.CanConvertToScp(path, detection, _getCapabilities())) return;
        var settings = _getSettings();
        if (string.IsNullOrWhiteSpace(settings.GwExecutablePath) || !File.Exists(settings.GwExecutablePath)) return;
        var formatId = detection.Format?.Id ?? "raw.scp";
        var temporaryPath = Path.Combine(Path.GetTempPath(), $"gwgui-visual-{Guid.NewGuid():N}.scp");
        string? stagedSourcePath = null;
        var gateEntered = false;
        try
        {
            await _cancellation.EnterVisualizationConversionAsync(cancellationToken);
            gateEntered = true;
            cancellationToken.ThrowIfCancellationRequested();
            var conversionSourcePath = path;
            if (Path.GetExtension(path).Equals(".atr", StringComparison.OrdinalIgnoreCase))
            {
                stagedSourcePath = Path.Combine(Path.GetTempPath(), $"gwgui-visual-{Guid.NewGuid():N}.img");
                await GWGUI.MediaEngine.Conversion.Atari.AtrPayloadWriter.WriteRawPayloadAsync(path, stagedSourcePath, cancellationToken);
                conversionSourcePath = stagedSourcePath;
            }
            var command = _commandBuilder.BuildConversion(settings.GwExecutablePath, conversionSourcePath, new ConversionOutput(formatId, ".scp", temporaryPath, false));
            var result = await _visualizationRunner.RunAsync(command, cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!result.IsSuccess || !File.Exists(temporaryPath)) return;
            await LoadScpAsync(temporaryPath, displayFileName ?? Path.GetFileName(path));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            if (gateEntered) _cancellation.ExitVisualizationConversion();
            TryDelete(stagedSourcePath);
            TryDelete(temporaryPath);
        }
    }

    public Task LoadScpAsync(string path, string? displayFileName = null) =>
        LoadScpCoreAsync(path, displayFileName, preserveProgress: false);

    private async Task LoadScpCoreAsync(
        string path,
        string? displayFileName,
        bool preserveProgress)
    {
        var cancellation = _cancellation.BeginScp();
        var cancellationToken = cancellation.Token;
        try
        {
            _scpDisplayName = displayFileName ?? Path.GetFileName(path);
            ShowProgress(_localize("Visual.Loading", []), 0, true);
            _visualizer.Header.SummaryText.Text = _localize("Visual.Loading", []);
            var document = await _scpLoader.LoadAsync(path, cancellationToken);
            _scpPath = path;
            _scpDisplayName = displayFileName ?? document.FileName;
            _scpSummary = document.Summary;
            ShowFluxDocument(path, document.Image);
            await DisplayScpAsync(document.Image, displayFileName ?? document.FileName, document.Summary, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            if (!_cancellation.IsCurrentScp(cancellation) || cancellationToken.IsCancellationRequested) return;
            _scpImage = null;
            _visualizer.Header.SummaryText.Text = _localize("Visual.Invalid", []);
            _showError(exception, $"Opening disk image in Visualizer: {path}", "Visual.Title", "Error.Unexpected");
        }
        finally
        {
            if (_cancellation.IsCurrentScp(cancellation) && !preserveProgress) HideProgress();
        }
    }

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
                && _explorerExploredImage?.SelectFormat(formatId) is { } explorerSelection)
            {
                _explorerExploredImage = explorerSelection;
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

        var explorerSelection = _explorerExploredImage?.SelectFormat(formatId);
        if (explorerSelection is null)
        {
            var explicitlyExplored = await LoadExplorerAsync(ExplorerPath, false);
            explorerSelection = explicitlyExplored?.SelectFormat(formatId);
            if (explorerSelection is null) return;
        }

        _explorerExploredImage = explorerSelection;
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
        && !string.IsNullOrWhiteSpace(_scpPath)
        && string.Equals(ExplorerPath, _scpPath, StringComparison.OrdinalIgnoreCase);

    private async Task SelectVisualizerRepresentationCoreAsync(string? formatId)
    {
        var sourcePath = VisualizerSourcePath();
        if (string.IsNullOrWhiteSpace(sourcePath)) return;
        var cancellation = _cancellation.BeginVisualization();
        var cancellationToken = cancellation.Token;
        if (string.IsNullOrWhiteSpace(formatId))
        {
            if (_scpImage is null || string.IsNullOrWhiteSpace(_scpPath)) return;
            ShowFluxDocument(_scpPath, _scpImage);
            await DisplayScpAsync(
                _scpImage,
                _scpDisplayName ?? Path.GetFileName(_scpPath),
                _scpSummary ?? string.Empty,
                cancellationToken);
            if (_cancellation.IsCurrentVisualization(cancellation)) HideProgress();
            return;
        }

        var selected = _visualizerExploredImage?.SelectFormat(formatId)
            ?? _explorerExploredImage?.SelectFormat(formatId)
            ?? await _explore(sourcePath, formatId, cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
        _visualizerExploredImage = selected;
        var document = new MediaImageDocument(
            new MediaSourceDescriptor(sourcePath, [], RequestedFormatId: formatId),
            selected.Image.FormatId,
            MediaKind.Floppy,
            new SectorMediaImageRepresentation(selected.Image),
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal));
        var descriptor = _visualizationProviders?.CreateDescriptor(document);
        if (descriptor is null) return;
        var geometry = _sectorPresenter.BuildGeometryModel(selected.Image);
        _sectorView.SetDocument(geometry, descriptor);
        _visualizer.ShowDocument(document, descriptor);
        var lastPresentation = System.Diagnostics.Stopwatch.GetTimestamp();
        foreach (var position in geometry.Surfaces
                     .SelectMany(surface => surface.Tracks.Select(track => (Surface: surface.Index, track.Cylinder)))
                     .OrderBy(item => item.Cylinder)
                     .ThenBy(item => item.Surface))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var track = _sectorPresenter.AnalyzeTrack(selected.Image, position.Surface, position.Cylinder);
            var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(lastPresentation);
            var remaining = RemainingSectorTrackPresentationDelay(elapsed);
            if (remaining > TimeSpan.Zero) await Task.Delay(remaining, cancellationToken);
            await _visualizer.Dispatcher.InvokeAsync(() =>
            {
                _sectorView.RevealTrack(position.Surface, track);
                _visualizer.Overview.MarkSectorTrack(position.Surface, track);
            }, DispatcherPriority.Background, cancellationToken);
            lastPresentation = System.Diagnostics.Stopwatch.GetTimestamp();
        }
    }

    private string? VisualizerSourcePath() =>
        _scpPath
        ?? _visualizerExploredImage?.SourcePath
        ?? _explorerExploredImage?.SourcePath
        ?? _exploredMediaImage?.Document.Source.PrimaryPath
        ?? ExplorerPath;

    private void ShowFluxDocument(string path, ScpImage image)
    {
        if (_visualizationProviders is null) return;
        var document = new MediaImageDocument(
            new MediaSourceDescriptor(path, []),
            DiskImageFormatIds.RawScp,
            MediaKind.Floppy,
            new FluxMediaImageRepresentation(ScpProtectedTrackImageAdapter.Create(image)),
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal));
        var descriptor = _visualizationProviders.CreateDescriptor(document);
        _visualizationDescriptor = descriptor;
        _visualizer.ShowDocument(document, descriptor);
    }

    public void ClearVisualizer(string fileName)
    {
        _cancellation.CancelScp();
        _scpImage = null;
        _scpPath = null;
        _scpDisplayName = fileName;
        _scpSummary = null;
        _scpTrackPreparations.Clear();
        _scpTrackPresentationOrder.Clear();
        _visualizerExploredImage = null;
        _visualizationDescriptor = null;
        _inspector.ClearImage();
        _visualizer.ClearDocumentForLoading();
        _visualizer.Header.FileNameText.Text = fileName;
        _visualizer.Header.SummaryText.Text = _localize("Visual.NoFile", []);
        _visualizer.FirstSide.SetImage(null, 0);
        _visualizer.SecondSide.SetImage(null, 1);
        _visualizer.Header.ClearRevolutions();
        _visualizer.Overview.Configure(new Dictionary<int, IReadOnlyList<int>>());
        _face0Progress.Reset();
        _face1Progress.Reset();
        HideProgress();
    }

    public void CancelAll() => _cancellation.CancelAll();

    public Task PrepareViewsForInspectorAsync(CancellationToken cancellationToken) => PrepareViewsAsync(cancellationToken);

    public void HideProgressForInspector() => HideProgress();

    public void Dispose() => _cancellation.Dispose();

    private async Task DisplayScpAsync(ScpImage image, string fileName, string summary, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        _scpImage = image;
        _scpTrackPreparations.Clear();
        _scpTrackPresentationOrder.Clear();
        _visualizer.Header.FileNameText.Text = fileName;
        var heads = image.Tracks.Select(track => track.Head).ToHashSet();
        _visualizer.Header.SummaryText.Text = summary;
        _visualizer.FirstSide.SetImage(image, 0);
        _visualizer.SecondSide.SetImage(image, 1);
        _visualizer.Header.ConfigureRevolutions(image.Tracks.Count == 0
            ? 0
            : image.Tracks.Max(track => track.Revolutions.Count));
        _inspector.SetImage(image);
        _visualizer.FirstSide.Visibility = heads.Contains(0) ? Visibility.Visible : Visibility.Collapsed;
        _visualizer.SecondSide.Visibility = heads.Contains(1) ? Visibility.Visible : Visibility.Collapsed;
        Grid.SetColumn(_visualizer.FirstSide, 0);
        Grid.SetColumnSpan(_visualizer.FirstSide, heads.Count == 1 && heads.Contains(0) ? 2 : 1);
        Grid.SetColumn(_visualizer.SecondSide, heads.Count == 1 && heads.Contains(1) ? 0 : 1);
        Grid.SetColumnSpan(_visualizer.SecondSide, heads.Count == 1 && heads.Contains(1) ? 2 : 1);
        await PrepareViewsAsync(cancellationToken);
    }

    private async Task PrepareViewsAsync(CancellationToken cancellationToken)
    {
        if (_scpImage is null) return;
        var heads = _scpImage.Tracks.Select(track => track.Head).Distinct().Order().ToArray();
        var total = Math.Max(1, _scpImage.Tracks.Count);
        var completedByHead = heads.ToDictionary(head => head, _ => 0);
        var cylindersByHead = heads.ToDictionary(head => head, head =>
            (IReadOnlyList<int>)_scpImage.Tracks.Where(track => track.Head == head).OrderBy(track => track.Cylinder).Select(track => track.Cylinder).ToArray());
        var presentationOrder = _scpImage.Tracks
            .OrderBy(track => track.Cylinder)
            .ThenBy(track => track.Head)
            .Select(track => (track.Head, track.Cylinder))
            .ToArray();
        _visualizer.Overview.Configure(cylindersByHead);
        _face0Progress.Configure(0, cylindersByHead.GetValueOrDefault(0) ?? [], _localize("Visual.Side", [0]));
        _face1Progress.Configure(1, cylindersByHead.GetValueOrDefault(1) ?? [], _localize("Visual.Side", [1]));
        _viewModel.ProgressVisibility = Visibility.Visible;
        _viewModel.GlobalProgressVisibility = Visibility.Collapsed;
        _viewModel.Face0ProgressVisibility = heads.Contains(0) ? Visibility.Visible : Visibility.Collapsed;
        _viewModel.Face1ProgressVisibility = heads.Contains(1) ? Visibility.Visible : Visibility.Collapsed;
        _viewModel.ProgressText = _localize("Visual.AnalysingTrack", [0, total]);
        var presentationQueue = Channel.CreateUnbounded<ScpTrackPreparation>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        var presentationTask = PresentPreparedTracksAsync();
        var preparations = heads.Select(head =>
        {
            var view = head == 0 ? _visualizer.FirstSide : _visualizer.SecondSide;
            var progress = new ImmediateProgress<ScpTrackPreparation>(preparation =>
            {
                if (cancellationToken.IsCancellationRequested) return;
                presentationQueue.Writer.TryWrite(preparation);
            });
            return view.PrepareAsync(progress, cancellationToken);
        });

        Exception? preparationFailure = null;
        try
        {
            await Task.WhenAll(preparations);
        }
        catch (Exception error)
        {
            preparationFailure = error;
        }
        finally
        {
            presentationQueue.Writer.TryComplete(preparationFailure);
        }
        await presentationTask;

        async Task PresentPreparedTracksAsync()
        {
            var first = true;
            var lastPresentation = System.Diagnostics.Stopwatch.GetTimestamp();
            var pending = new Dictionary<(int Head, int Cylinder), ScpTrackPreparation>();
            var next = 0;
            await foreach (var preparation in presentationQueue.Reader.ReadAllAsync(cancellationToken))
            {
                pending[(preparation.Head, preparation.Cylinder)] = preparation;
                while (next < presentationOrder.Length && pending.Remove(presentationOrder[next], out var ordered))
                {
                    if (!first)
                    {
                        var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(lastPresentation);
                        var remaining = MinimumTrackPresentationInterval - elapsed;
                        if (remaining > TimeSpan.Zero) await Task.Delay(remaining, cancellationToken);
                    }
                    first = false;
                    await PresentTrackAsync(ordered);
                    lastPresentation = System.Diagnostics.Stopwatch.GetTimestamp();
                    next++;
                }
            }

            async Task PresentTrackAsync(ScpTrackPreparation preparation)
            {
                await _visualizer.Dispatcher.InvokeAsync(() =>
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    var view = preparation.Head == 0 ? _visualizer.FirstSide : _visualizer.SecondSide;
                    var strip = preparation.Head == 0 ? _face0Progress : _face1Progress;
                    _scpTrackPreparations[(preparation.Head, preparation.Cylinder)] = preparation;
                    _scpTrackPresentationOrder.Add((preparation.Head, preparation.Cylinder));
                    completedByHead[preparation.Head]++;
                    var current = Math.Min(total, completedByHead.Values.Sum());
                    strip.SetState(preparation.Cylinder, TrackSegmentState.Success);
                    strip.ClearActive();
                    _visualizer.Overview.MarkPrepared(preparation);
                    _viewModel.ProgressText = _localize("Visual.AnalysingTrack", [current, total]);
                    view.RevealPreparedTrack(preparation.Cylinder);
                }, DispatcherPriority.Background, cancellationToken);
            }
        }
    }

    internal sealed class ImmediateProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    private void RestoreFluxOverview()
    {
        if (_scpImage is null) return;
        var cylindersByHead = _scpImage.Tracks
            .GroupBy(track => track.Head)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<int>)group.OrderBy(track => track.Cylinder).Select(track => track.Cylinder).ToArray());
        _visualizer.Overview.Configure(cylindersByHead);
        foreach (var preparation in _scpTrackPreparations.Values.OrderBy(item => item.Head).ThenBy(item => item.Cylinder))
            _visualizer.Overview.MarkPrepared(preparation);
    }

    private void ShowProgress(string text, double value, bool indeterminate)
    {
        _viewModel.ProgressText = string.Empty;
        _viewModel.ProgressValue = value;
        _viewModel.ProgressIndeterminate = indeterminate;
        _viewModel.ProgressVisibility = Visibility.Visible;
        _viewModel.GlobalProgressVisibility = Visibility.Visible;
        _viewModel.Face0ProgressVisibility = Visibility.Collapsed;
        _viewModel.Face1ProgressVisibility = Visibility.Collapsed;
        _viewModel.Face0ProgressValue = 0;
        _viewModel.Face1ProgressValue = 0;
        _face0Progress.Reset();
        _face1Progress.Reset();
        _visualizer.SetRecognitionProgress(true, text, _scpDisplayName ?? string.Empty, value, indeterminate);
        _viewModel.OperationText = _localize("Tab.Read", []);
        _viewModel.OperationBrush = Brushes.SeaGreen;
    }

    internal void ReportSharedProgress(string stage, string detail, double value, bool indeterminate)
    {
        _viewModel.ProgressText = string.Empty;
        _viewModel.ProgressValue = value;
        _viewModel.ProgressIndeterminate = indeterminate;
        _viewModel.ProgressVisibility = Visibility.Visible;
        if (_scpImage is null)
        {
            _viewModel.GlobalProgressVisibility = Visibility.Visible;
            _viewModel.Face0ProgressVisibility = Visibility.Collapsed;
            _viewModel.Face1ProgressVisibility = Visibility.Collapsed;
        }
        _visualizer.SetRecognitionProgress(true, stage, detail, value, indeterminate);
        _viewModel.OperationText = _localize("Tab.Read", []);
        _viewModel.OperationBrush = Brushes.SeaGreen;
    }

    private void HideProgress()
    {
        if (_operationIsRunning()) return;
        _viewModel.ProgressVisibility = Visibility.Collapsed;
        _viewModel.ProgressIndeterminate = false;
        _visualizer.SetRecognitionProgress(false, string.Empty, string.Empty, 0);
        _viewModel.OperationText = _localize("Status.ReadyShort", []);
        _viewModel.OperationBrush = Brushes.Gray;
    }

    private async Task<ExploredMediaImage?> ExploreMediaAsync(
        string path,
        string? requestedFormatId,
        CancellationToken cancellationToken,
        Action<string, string, double>? reportProgress = null)
    {
        if (_mediaReader is null || _mediaExplorer is null) return null;

        reportProgress?.Invoke(_localize("Explorer.LoadingMedia", []), Path.GetFileName(path), 58);
        var source = new MediaSourceDescriptor(path, [], RequestedFormatId: requestedFormatId);
        MediaImageDocument document;
        try
        {
            document = await _mediaReader.ReadAsync(source, cancellationToken);
        }
        catch (NotSupportedException) when (requestedFormatId is not null)
        {
            document = await _mediaReader.ReadAsync(source with { RequestedFormatId = null }, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        reportProgress?.Invoke(_localize("Explorer.LoadingFileSystem", []), document.FormatId, 78);
        var explored = await _mediaExplorer.ExploreAsync(document, cancellationToken);
        reportProgress?.Invoke(_localize("Explorer.LoadingContents", []), explored.Document.FormatId, 94);
        return explored;
    }

    private async Task<bool> PresentMediaVisualizationAsync(
        MediaImageDocument document,
        CancellationToken cancellationToken)
    {
        _visualizationDescriptor = _visualizationProviders?.CreateDescriptor(document);
        if (_visualizationDescriptor is null) return false;
        if (document.Representation is SectorMediaImageRepresentation sectorRepresentation)
        {
            var geometry = _sectorPresenter.BuildGeometryModel(sectorRepresentation.Image);
            var cylindersBySurface = geometry.Surfaces.ToDictionary(
                surface => surface.Index,
                surface => (IReadOnlyList<int>)surface.Tracks.Select(track => track.Cylinder).ToArray());
            _face0Progress.Configure(0, cylindersBySurface.GetValueOrDefault(0) ?? [], _localize("Visual.Side", [0]));
            _face1Progress.Configure(1, cylindersBySurface.GetValueOrDefault(1) ?? [], _localize("Visual.Side", [1]));
            _viewModel.ProgressVisibility = Visibility.Visible;
            _viewModel.GlobalProgressVisibility = Visibility.Collapsed;
            _viewModel.Face0ProgressVisibility = cylindersBySurface.ContainsKey(0) ? Visibility.Visible : Visibility.Collapsed;
            _viewModel.Face1ProgressVisibility = cylindersBySurface.ContainsKey(1) ? Visibility.Visible : Visibility.Collapsed;
            _viewModel.ProgressIndeterminate = false;
            _viewModel.ProgressText = string.Empty;
            _sectorView.SetDocument(geometry, _visualizationDescriptor);
            _visualizer.ShowDocument(document, _visualizationDescriptor);
            var lastPresentation = System.Diagnostics.Stopwatch.GetTimestamp();
            foreach (var position in geometry.Surfaces
                         .SelectMany(surface => surface.Tracks.Select(track => (Surface: surface.Index, track.Cylinder)))
                         .OrderBy(item => item.Cylinder)
                         .ThenBy(item => item.Surface))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var track = _sectorPresenter.AnalyzeTrack(sectorRepresentation.Image, position.Surface, position.Cylinder);
                var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(lastPresentation);
                var remaining = RemainingSectorTrackPresentationDelay(elapsed);
                if (remaining > TimeSpan.Zero) await Task.Delay(remaining, cancellationToken);
                await _visualizer.Dispatcher.InvokeAsync(() =>
                {
                    _sectorView.RevealTrack(position.Surface, track);
                    _visualizer.Overview.MarkSectorTrack(position.Surface, track);
                    var progress = position.Surface == 0 ? _face0Progress : _face1Progress;
                    progress.SetState(position.Cylinder, TrackSegmentState.Success);
                }, DispatcherPriority.Background, cancellationToken);
                lastPresentation = System.Diagnostics.Stopwatch.GetTimestamp();
            }
        }
        else
        {
            if (document.Representation is GWGUI.MediaEngine.Representations.Blocks.BlockMediaImageRepresentation)
            {
                _blockRenderModel = _blockPresenter.BuildRenderModel(document);
                _selectedBlockRange = null;
                _selectedBlockSurface = null;
                _blockView.SetDocument(_blockRenderModel);
            }
            else if (document.Representation is GWGUI.MediaEngine.Representations.Optical.OpticalMediaImageRepresentation)
            {
                _opticalDocument = document;
                _opticalRenderModel = _opticalPresenter.BuildRenderModel(document);
                _opticalView.SetDocument(_opticalRenderModel);
            }
            else if (document.Representation is GWGUI.MediaEngine.Representations.Sequential.SequentialMediaImageRepresentation)
            {
                _sequentialRenderModel = _sequentialPresenter.BuildRenderModel(document);
                _sequentialView.SetDocument(_sequentialRenderModel);
            }
            _visualizer.ShowDocument(document, _visualizationDescriptor);
        }
        return _visualizationDescriptor.RepresentationKind != MediaRepresentationKind.Flux;
    }

    internal static TimeSpan RemainingSectorTrackPresentationDelay(TimeSpan elapsedSincePresentation)
    {
        var remaining = MinimumSectorTrackPresentationInterval - elapsedSincePresentation;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    private void HandleSectorSelected(int surface, GWGUI.App.Contracts.Rendering.Sectors.SectorMediaElement? sector) =>
        _visualizer.SetInspectorModel(surface, sector is null ? null : _sectorPresenter.BuildInspectorModel(surface, sector));

    private void HandleBlockRangeSelected(GWGUI.App.Contracts.Rendering.Blocks.BlockMediaRange? range)
    {
        _selectedBlockRange = range;
        UpdateBlockInspector();
    }

    private void HandleBlockSurfaceSelected(int? surface)
    {
        _selectedBlockSurface = surface;
        UpdateBlockInspector();
    }

    private void UpdateBlockInspector() => _visualizer.SetInspectorModel(_blockRenderModel is null
        ? null
        : _blockPresenter.BuildInspectorModel(_blockRenderModel, _selectedBlockRange, _selectedBlockSurface));

    private void HandleOpticalTrackSelected(GWGUI.App.Contracts.Rendering.Optical.OpticalMediaTrack? track) =>
        _visualizer.SetInspectorModel(_opticalDocument is null || _opticalRenderModel is null
            ? null
            : _opticalPresenter.BuildInspectorModel(_opticalDocument, _opticalRenderModel, track));

    private void HandleSequentialSegmentSelected(GWGUI.App.Contracts.Rendering.Sequential.SequentialMediaSegment? segment) =>
        _visualizer.SetInspectorModel(_sequentialRenderModel is null
            ? null
            : _sequentialPresenter.BuildInspectorModel(_sequentialRenderModel, segment));

    private static void TryDelete(string? path)
    {
        try { if (path is not null && File.Exists(path)) File.Delete(path); }
        catch { }
    }
}
