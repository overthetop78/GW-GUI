using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.App.Contracts.Progress;
using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Contracts.Visualization;
using GWGUI.App.Functions.Services.Visualization;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Services.Visualization;
using GWGUI.App.Presenters.Visualization;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaEngine.Visualization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using GWGUI.Infrastructure.Processes;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Contracts;


using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.App.Services.DiskImages;

internal sealed class DiskImageWorkspaceController : IDisposable
{
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
    private ExploredDiskImage? _exploredImage;
    private ExploredMediaImage? _exploredMediaImage;
    private MediaVisualizationDescriptor? _visualizationDescriptor;
    private GWGUI.App.Contracts.Rendering.Blocks.BlockMediaRenderModel? _blockRenderModel;
    private GWGUI.App.Contracts.Rendering.Blocks.BlockMediaRange? _selectedBlockRange;
    private int? _selectedBlockSurface;
    private MediaImageDocument? _opticalDocument;
    private GWGUI.App.Contracts.Rendering.Optical.OpticalMediaRenderModel? _opticalRenderModel;
    private GWGUI.App.Contracts.Rendering.Sequential.SequentialMediaRenderModel? _sequentialRenderModel;

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
        LastReadImage = document;
        return document;
    }

    public void RememberReadImage(IImageDisquette image)
    {
        ArgumentNullException.ThrowIfNull(image);
        LastReadImage = image;
    }

    public string? SelectImage()
    {
        var settings = _getSettings();
        var initialDirectory = !string.IsNullOrWhiteSpace(settings.LastDiskImageFolder) && Directory.Exists(settings.LastDiskImageFolder)
            ? settings.LastDiskImageFolder
            : settings.DefaultImagesFolder;
        var path = _fileDialogs.OpenFile(new(_localize("Common.DiskImageFilter", []), initialDirectory));
        if (path is not null) settings.LastDiskImageFolder = Path.GetDirectoryName(path);
        return path;
    }

    public async Task LoadAsync(string path, string? displayFileName = null)
    {
        var explored = await LoadExplorerAsync(path, true);
        if (explored is null) return;
        await LoadVisualizerAsync(path, displayFileName, explored);
    }

    public async Task<ExploredDiskImage?> LoadExplorerAsync(string path, bool newImage = true)
    {
        var cancellation = _cancellation.BeginExplorer();
        ExplorerPath = path;
        _explorer.Clear(path, newImage);
        _explorer.SetLoading(true);
        try
        {
            var requestedFormat = newImage ? _explorer.FormatIdForNewImage : _explorer.SelectedFormatId;
            var document = SelectCachedInterpretation(path, newImage, requestedFormat);
            document ??= await ExploreWithSelectedEngineAsync(path, requestedFormat, cancellation.Token);
            if (!cancellation.IsCancellationRequested)
            {
                _exploredImage = document;
                _exploredMediaImage = await ExploreMediaAsync(path, requestedFormat, cancellation.Token);
                LastReadImage = document;
                if (_exploredMediaImage?.Document.Representation is SectorMediaImageRepresentation)
                    _explorer.Display(_exploredMediaImage);
                else
                    _explorer.Display(document);
                var detectedFormatIds = document.FormatsDetectes
                    .Select(format => format.FormatId)
                    .ToArray();
                _visualizer.Header.ApplyDetection(
                    document.PrimaryFormatId,
                    document.Metadata.ProtectionId,
                    detectedFormatIds);
                ApplyClassification();
            }
            return cancellation.IsCancellationRequested ? null : document;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { return null; }
        catch (Exception exception)
        {
            if (!_cancellation.IsCurrentExplorer(cancellation) || cancellation.IsCancellationRequested) return null;
            if (_cancellation.IsCurrentExplorer(cancellation)) _explorer.SetLoading(false);
            _showError(exception, $"Opening disk image in Explorer: {path}", "Tab.Explorer", "Explorer.LoadFailed");
            return null;
        }
        finally
        {
            if (_cancellation.IsCurrentExplorer(cancellation)) _explorer.SetLoading(false);
        }
    }

    private ExploredDiskImage? SelectCachedInterpretation(
        string path,
        bool newImage,
        string? requestedFormat)
    {
        if (newImage || string.IsNullOrWhiteSpace(requestedFormat) || _exploredImage is null)
        {
            return null;
        }

        if (!string.Equals(_exploredImage.SourcePath, path, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return _exploredImage.SelectFormat(requestedFormat);
    }

    private async Task<ExploredDiskImage> ExploreWithSelectedEngineAsync(
        string path,
        string? requestedFormat,
        CancellationToken cancellationToken)
    {
        var settings = _getSettings();
        if (settings.Engines.ExplorerRead == OperationEngine.Internal)
            return await _explore(path, requestedFormat, cancellationToken);

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
        var explored = exploredImage;
        try
        {
            if (explored is null)
            {
                explored = await AnalyzeAsync(path, cancellation.Token);
            }
            else
            {
                LastReadImage = explored;
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        {
            explored = null;
        }

        if (cancellation.IsCancellationRequested) return;
        if (Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            await LoadScpAsync(path, displayFileName);
            return;
        }

        if (_exploredMediaImage is not null
            && _visualizationDescriptor?.RepresentationKind is { } representationKind
            && representationKind != MediaRepresentationKind.Flux)
        {
            _scpImage = null;
            _inspector.ClearImage();
            HideProgress();
            return;
        }

        ClearVisualizer(displayFileName ?? Path.GetFileName(path));
        if (_operationIsRunning()) return;
        cancellation.Token.ThrowIfCancellationRequested();

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
            await _cancellation.EnterVisualizationConversionAsync(cancellation.Token);
            gateEntered = true;
            cancellation.Token.ThrowIfCancellationRequested();
            var conversionSourcePath = path;
            if (Path.GetExtension(path).Equals(".atr", StringComparison.OrdinalIgnoreCase))
            {
                stagedSourcePath = Path.Combine(Path.GetTempPath(), $"gwgui-visual-{Guid.NewGuid():N}.img");
                await GWGUI.MediaEngine.Conversion.Atari.AtrPayloadWriter.WriteRawPayloadAsync(path, stagedSourcePath, cancellation.Token);
                conversionSourcePath = stagedSourcePath;
            }
            var command = _commandBuilder.BuildConversion(settings.GwExecutablePath, conversionSourcePath, new ConversionOutput(formatId, ".scp", temporaryPath, false));
            var result = await _visualizationRunner.RunAsync(command, cancellationToken: cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            if (!result.IsSuccess || !File.Exists(temporaryPath)) return;
            await LoadScpAsync(temporaryPath, displayFileName ?? Path.GetFileName(path));
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        finally
        {
            if (gateEntered) _cancellation.ExitVisualizationConversion();
            TryDelete(stagedSourcePath);
            TryDelete(temporaryPath);
        }
    }

    public async Task LoadScpAsync(string path, string? displayFileName = null)
    {
        var cancellation = _cancellation.BeginScp();
        try
        {
            ShowProgress(_localize("Visual.Loading", []), 0, true);
            _visualizer.Header.SummaryText.Text = _localize("Visual.Loading", []);
            var document = await _scpLoader.LoadAsync(path, cancellation.Token);
            await DisplayScpAsync(document.Image, displayFileName ?? document.FileName, document.Summary, cancellation);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        catch (Exception exception)
        {
            if (!_cancellation.IsCurrentScp(cancellation) || cancellation.IsCancellationRequested) return;
            _scpImage = null;
            _visualizer.Header.SummaryText.Text = _localize("Visual.Invalid", []);
            _showError(exception, $"Opening disk image in Visualizer: {path}", "Visual.Title", "Error.Unexpected");
        }
        finally
        {
            if (_cancellation.IsCurrentScp(cancellation)) HideProgress();
        }
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

    public void ClearVisualizer(string fileName)
    {
        _cancellation.CancelScp();
        _scpImage = null;
        _inspector.ClearImage();
        _visualizer.Header.FileNameText.Text = fileName;
        _visualizer.Header.SummaryText.Text = _localize("Visual.NoFile", []);
        _visualizer.FirstSide.SetImage(null, 0);
        _visualizer.SecondSide.SetImage(null, 1);
        _visualizer.Overview.Configure(new Dictionary<int, IReadOnlyList<int>>());
        _face0Progress.Reset();
        _face1Progress.Reset();
        HideProgress();
    }

    public void CancelAll() => _cancellation.CancelAll();

    public Task PrepareViewsForInspectorAsync(CancellationToken cancellationToken) => PrepareViewsAsync(cancellationToken);

    public void HideProgressForInspector() => HideProgress();

    public void Dispose() => _cancellation.Dispose();

    private async Task DisplayScpAsync(ScpImage image, string fileName, string summary, CancellationTokenSource cancellation)
    {
        if (cancellation.IsCancellationRequested) return;
        _scpImage = image;
        _visualizer.Header.FileNameText.Text = fileName;
        var heads = image.Tracks.Select(track => track.Head).ToHashSet();
        _visualizer.Header.SummaryText.Text = summary;
        _visualizer.FirstSide.SetImage(image, 0);
        _visualizer.SecondSide.SetImage(image, 1);
        _inspector.SetImage(image);
        _visualizer.FirstSide.Visibility = heads.Contains(0) ? Visibility.Visible : Visibility.Collapsed;
        _visualizer.SecondSide.Visibility = heads.Contains(1) ? Visibility.Visible : Visibility.Collapsed;
        Grid.SetColumn(_visualizer.FirstSide, 0);
        Grid.SetColumnSpan(_visualizer.FirstSide, heads.Count == 1 && heads.Contains(0) ? 2 : 1);
        Grid.SetColumn(_visualizer.SecondSide, heads.Count == 1 && heads.Contains(1) ? 0 : 1);
        Grid.SetColumnSpan(_visualizer.SecondSide, heads.Count == 1 && heads.Contains(1) ? 2 : 1);
        await PrepareViewsAsync(cancellation.Token);
    }

    private async Task PrepareViewsAsync(CancellationToken cancellationToken)
    {
        if (_scpImage is null) return;
        var heads = _scpImage.Tracks.Select(track => track.Head).Distinct().Order().ToArray();
        var total = Math.Max(1, _scpImage.Tracks.Count);
        var completedByHead = heads.ToDictionary(head => head, _ => 0);
        var cylindersByHead = heads.ToDictionary(head => head, head =>
            (IReadOnlyList<int>)_scpImage.Tracks.Where(track => track.Head == head).OrderBy(track => track.Cylinder).Select(track => track.Cylinder).ToArray());
        _visualizer.Overview.Configure(cylindersByHead);
        _face0Progress.Configure(0, cylindersByHead.GetValueOrDefault(0) ?? [], _localize("Visual.Side", [0]));
        _face1Progress.Configure(1, cylindersByHead.GetValueOrDefault(1) ?? [], _localize("Visual.Side", [1]));
        _viewModel.ProgressVisibility = Visibility.Visible;
        _viewModel.GlobalProgressVisibility = Visibility.Collapsed;
        _viewModel.Face0ProgressVisibility = heads.Contains(0) ? Visibility.Visible : Visibility.Collapsed;
        _viewModel.Face1ProgressVisibility = heads.Contains(1) ? Visibility.Visible : Visibility.Collapsed;
        _viewModel.ProgressText = _localize("Visual.AnalysingTrack", [0, total]);
        var preparations = heads.Select(head =>
        {
            var view = head == 0 ? _visualizer.FirstSide : _visualizer.SecondSide;
            var strip = head == 0 ? _face0Progress : _face1Progress;
            var cylinders = cylindersByHead[head];
            var progress = new Progress<ScpTrackPreparation>(preparation =>
            {
                if (cancellationToken.IsCancellationRequested) return;
                var value = ++completedByHead[head];
                var current = Math.Min(total, completedByHead.Values.Sum());
                for (var index = 0; index < Math.Min(value, cylinders.Count); index++) strip.SetState(cylinders[index], TrackSegmentState.Success);
                if (value < cylinders.Count) strip.SetActive(cylinders[value]); else strip.ClearActive();
                _visualizer.Overview.MarkPrepared(preparation);
                _viewModel.ProgressText = _localize("Visual.AnalysingTrack", [current, total]);
                view.RefreshPreparedTracks();
            });
            return view.PrepareAsync(progress, cancellationToken);
        });
        await Task.WhenAll(preparations);
    }

    private void ShowProgress(string text, double value, bool indeterminate)
    {
        _viewModel.ProgressText = text;
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
    }

    private void HideProgress()
    {
        if (_operationIsRunning()) return;
        _viewModel.ProgressVisibility = Visibility.Collapsed;
        _viewModel.ProgressIndeterminate = false;
    }

    private async Task<ExploredMediaImage?> ExploreMediaAsync(
        string path,
        string? requestedFormatId,
        CancellationToken cancellationToken)
    {
        if (_mediaReader is null || _mediaExplorer is null) return null;

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
        var explored = await _mediaExplorer.ExploreAsync(document, cancellationToken);
        document = explored.Document;
        _visualizationDescriptor = _visualizationProviders?.CreateDescriptor(document);
        if (_visualizationDescriptor is not null)
        {
            if (document.Representation is SectorMediaImageRepresentation sectors)
                _sectorView.SetDocument(_sectorPresenter.BuildRenderModel(sectors.Image), _visualizationDescriptor);
            else if (document.Representation is GWGUI.MediaEngine.Representations.Blocks.BlockMediaImageRepresentation)
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
        return explored;
    }

    private void HandleSectorSelected(int surface, GWGUI.App.Contracts.Rendering.Sectors.SectorMediaElement? sector) =>
        _visualizer.SetInspectorModel(sector is null ? null : _sectorPresenter.BuildInspectorModel(surface, sector));

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
