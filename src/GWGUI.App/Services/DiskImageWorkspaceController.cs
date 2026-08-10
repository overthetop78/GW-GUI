using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Controls;
using GWGUI.App.Rendering;
using GWGUI.App.ViewModels;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Write;
using GWGUI.Infrastructure.Processes;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Images;

namespace GWGUI.App.Services;

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
    private readonly DiskImageExplorer _diskImageExplorer;
    private readonly SectorImageFluxVisualizer _sectorVisualizer;
    private readonly Func<bool> _operationIsRunning;
    private readonly Action<Exception, string, string, string> _showError;
    private readonly Func<string, object[], string> _localize;
    private readonly DiskImageCancellationScope _cancellation;
    private ScpImage? _scpImage;

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
        SectorImageFluxVisualizer sectorVisualizer,
        DiskImageCancellationScope cancellation,
        Func<bool> operationIsRunning,
        Action<Exception, string, string, string> showError,
        Func<string, object[], string> localize)
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
        _sectorVisualizer = sectorVisualizer;
        _cancellation = cancellation;
        _operationIsRunning = operationIsRunning;
        _showError = showError;
        _localize = localize;
    }

    public string? ExplorerPath { get; private set; }
    public string? LastCapturedPath { get; set; }

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
        if (Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            await Task.WhenAll(LoadVisualizerAsync(path, displayFileName), LoadExplorerAsync(path));
            return;
        }

        var explored = await LoadExplorerAsync(path);
        await LoadVisualizerAsync(path, displayFileName, explored);
    }

    public async Task<ExploredDiskImage?> LoadExplorerAsync(string path)
    {
        var cancellation = _cancellation.BeginExplorer();
        ExplorerPath = path;
        _explorer.Clear(path);
        _explorer.SetLoading(true);
        try
        {
            var document = await _diskImageExplorer.ExploreAsync(path, _explorer.FormatIdForLoading, cancellation.Token);
            if (!cancellation.IsCancellationRequested)
            {
                _explorer.Display(document);
                _visualizer.Header.ApplyDetection(document.Image.FormatId,
                    document.Metadata.ProtectionName is null ? null : "apple2.rwts18");
                ApplyClassification();
            }
            return cancellation.IsCancellationRequested ? null : document;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { return null; }
        catch (Exception exception)
        {
            if (_cancellation.IsCurrentExplorer(cancellation)) _explorer.SetLoading(false);
            _showError(exception, $"Opening disk image in Explorer: {path}", "Tab.Explorer", "Explorer.LoadFailed");
            return null;
        }
        finally
        {
            if (_cancellation.IsCurrentExplorer(cancellation)) _explorer.SetLoading(false);
        }
    }

    public async Task LoadVisualizerAsync(string path, string? displayFileName = null, ExploredDiskImage? exploredImage = null)
    {
        var cancellation = _cancellation.BeginVisualization();
        if (Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            await LoadScpAsync(path, displayFileName);
            return;
        }

        ClearVisualizer(displayFileName ?? Path.GetFileName(path));
        if (_operationIsRunning()) return;
        try
        {
            var explored = exploredImage ?? await _diskImageExplorer.ExploreAsync(path, cancellationToken: cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            if (_sectorVisualizer.CanVisualize(explored.Image) && explored.Image.AvailableBlocks.Count > 0)
            {
                ShowProgress(_localize("Visual.Loading", []), 0, true);
                var visualization = await Task.Run(() => _sectorVisualizer.Create(explored.Image, cancellation.Token), cancellation.Token);
                var summary = $"{explored.Image.FormatId} · {explored.Image.Cylinders}×{explored.Image.Heads}×{explored.Image.SectorsPerTrack} · {explored.Image.AvailableBlocks.Count}/{explored.Image.BlockCount}";
                await DisplayScpAsync(visualization, displayFileName ?? Path.GetFileName(path), summary, cancellation);
                return;
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { return; }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException) { }

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
        var classification = DiskVisualizationClassificationPolicy.Resolve(
            selector.SelectedMachine,
            selector.SelectedFormatId,
            selector.SelectedProtectionId,
            selector.AutomaticDetection);
        var choice = decoder.Items.Cast<ScpDecoderChoice>()
            .FirstOrDefault(item => string.Equals(item.Id, classification.DecoderId, StringComparison.OrdinalIgnoreCase));
        if (choice is not null && !Equals(decoder.SelectedItem, choice)) decoder.SelectedItem = choice;
        _visualizer.FirstSide.SetMediaKind(classification.MediaKind);
        _visualizer.SecondSide.SetMediaKind(classification.MediaKind);
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

    private static void TryDelete(string? path)
    {
        try { if (path is not null && File.Exists(path)) File.Delete(path); }
        catch { }
    }
}
