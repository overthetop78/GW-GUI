using GWGUI.App.Contracts.Progress;
using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Constants.DiskImages;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Services.Visualization;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Domain.Enums;
using GWGUI.Domain.Contracts;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Representations.Flux;
using GWGUI.MediaEngine.Visualization;
using System.IO;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace GWGUI.App.Services.DiskImages.Visualization;

internal sealed class ScpVisualizationController
{
    private static readonly TimeSpan MinimumTrackPresentationInterval = TimeSpan.FromMilliseconds(30);

    private readonly VisualizerTabSection _visualizer;
    private readonly MainWindowViewModel _viewModel;
    private readonly TrackProgressStrip _face0Progress;
    private readonly TrackProgressStrip _face1Progress;
    private readonly ScpInspectorController _inspector;
    private readonly ScpDocumentLoader _loader;
    private readonly DiskImageCancellationScope _cancellation;
    private readonly MediaVisualizationProviderRegistry? _visualizationProviders;
    private readonly Func<bool> _operationIsRunning;
    private readonly Action<Exception, string, string, string> _showError;
    private readonly Func<string, object[], string> _localize;
    private readonly Dictionary<(int Head, int Cylinder), ScpTrackPreparation> _trackPreparations = [];
    private readonly List<(int Head, int Cylinder)> _trackPresentationOrder = [];

    internal ScpVisualizationController(
        VisualizerTabSection visualizer,
        MainWindowViewModel viewModel,
        TrackProgressStrip face0Progress,
        TrackProgressStrip face1Progress,
        ScpInspectorController inspector,
        ScpDocumentLoader loader,
        DiskImageCancellationScope cancellation,
        MediaVisualizationProviderRegistry? visualizationProviders,
        Func<bool> operationIsRunning,
        Action<Exception, string, string, string> showError,
        Func<string, object[], string> localize)
    {
        _visualizer = visualizer;
        _viewModel = viewModel;
        _face0Progress = face0Progress;
        _face1Progress = face1Progress;
        _inspector = inspector;
        _loader = loader;
        _cancellation = cancellation;
        _visualizationProviders = visualizationProviders;
        _operationIsRunning = operationIsRunning;
        _showError = showError;
        _localize = localize;
    }

    internal ScpImage? Image { get; private set; }
    internal string? SourcePath { get; private set; }
    internal string? DisplayName { get; private set; }
    internal string? Summary { get; private set; }
    internal IReadOnlyList<(int Head, int Cylinder)> TrackPresentationOrder => _trackPresentationOrder;

    internal Task LoadAsync(string path, string? displayFileName = null, bool preserveProgress = false) =>
        LoadCoreAsync(path, displayFileName, preserveProgress);

    internal async Task RestoreFluxRepresentationAsync(CancellationToken cancellationToken)
    {
        if (Image is null || string.IsNullOrWhiteSpace(SourcePath)) return;
        ShowFluxDocument(SourcePath, Image);
        await DisplayAsync(
            Image,
            DisplayName ?? Path.GetFileName(SourcePath),
            Summary ?? string.Empty,
            cancellationToken);
    }

    internal void ResetForNonScp()
    {
        Image = null;
        SourcePath = null;
        DisplayName = null;
        Summary = null;
        _trackPreparations.Clear();
        _trackPresentationOrder.Clear();
        _inspector.ClearImage();
        _visualizer.Header.ClearRevolutions();
    }

    internal void Clear(string fileName)
    {
        _cancellation.CancelScp();
        Image = null;
        SourcePath = null;
        DisplayName = fileName;
        Summary = null;
        _trackPreparations.Clear();
        _trackPresentationOrder.Clear();
        _inspector.ClearImage();
        _visualizer.ClearDocumentForLoading();
        _visualizer.Header.FileNameText.Text = fileName;
        _visualizer.Header.SummaryText.Text = _localize(DiskImageResourceKeys.VisualNoFile, []);
        _visualizer.FirstSide.SetImage(null, DiskHeadConstants.Head0);
        _visualizer.SecondSide.SetImage(null, DiskHeadConstants.Head1);
        _visualizer.Header.ClearRevolutions();
        _visualizer.Overview.Configure(new Dictionary<int, IReadOnlyList<int>>());
        _face0Progress.Reset();
        _face1Progress.Reset();
        HideProgress();
    }

    internal void ReportSharedProgress(string stage, string detail, double value, bool indeterminate)
    {
        _viewModel.ProgressText = string.Empty;
        _viewModel.ProgressValue = value;
        _viewModel.ProgressIndeterminate = indeterminate;
        if (Image is null)
        {
            _viewModel.ProgressVisibility = Visibility.Collapsed;
            _viewModel.GlobalProgressVisibility = Visibility.Collapsed;
            _viewModel.Face0ProgressVisibility = Visibility.Collapsed;
            _viewModel.Face1ProgressVisibility = Visibility.Collapsed;
        }
        _visualizer.SetRecognitionProgress(true, stage, detail, value, indeterminate);
        _viewModel.OperationText = _localize(DiskImageResourceKeys.TabRead, []);
        _viewModel.OperationBrush = Brushes.SeaGreen;
    }

    internal void HideProgress()
    {
        if (_operationIsRunning()) return;
        _viewModel.ProgressVisibility = Visibility.Collapsed;
        _viewModel.ProgressIndeterminate = false;
        _visualizer.SetRecognitionProgress(false, string.Empty, string.Empty, 0);
        _viewModel.OperationText = _localize(DiskImageResourceKeys.StatusReadyShort, []);
        _viewModel.OperationBrush = Brushes.Gray;
    }

    internal Task PrepareViewsAsync(CancellationToken cancellationToken) => PrepareTracksAsync(cancellationToken);

    private async Task LoadCoreAsync(string path, string? displayFileName, bool preserveProgress)
    {
        var cancellation = _cancellation.BeginScp();
        var cancellationToken = cancellation.Token;
        try
        {
            DisplayName = displayFileName ?? Path.GetFileName(path);
            ShowProgress(_localize(DiskImageResourceKeys.VisualLoading, []), 0, true);
            _visualizer.Header.SummaryText.Text = _localize(DiskImageResourceKeys.VisualLoading, []);
            var document = await _loader.LoadAsync(path, cancellationToken);
            SourcePath = path;
            DisplayName = displayFileName ?? document.FileName;
            Summary = document.Summary;
            ShowFluxDocument(path, document.Image);
            await DisplayAsync(document.Image, displayFileName ?? document.FileName, document.Summary, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            if (!_cancellation.IsCurrentScp(cancellation) || cancellationToken.IsCancellationRequested) return;
            Image = null;
            _visualizer.Header.SummaryText.Text = _localize(DiskImageResourceKeys.VisualInvalid, []);
            _showError(
                exception,
                $"Opening disk image in Visualizer: {path}",
                DiskImageResourceKeys.VisualTitle,
                DiskImageResourceKeys.ErrorUnexpected);
        }
        finally
        {
            if (_cancellation.IsCurrentScp(cancellation) && !preserveProgress) HideProgress();
        }
    }

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
        _visualizer.ShowDocument(document, descriptor);
    }

    private async Task DisplayAsync(
        ScpImage image,
        string fileName,
        string summary,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        Image = image;
        _trackPreparations.Clear();
        _trackPresentationOrder.Clear();
        _visualizer.Header.FileNameText.Text = fileName;
        var heads = image.Tracks.Select(track => track.Head).ToHashSet();
        _visualizer.Header.SummaryText.Text = summary;
        _visualizer.FirstSide.SetImage(image, DiskHeadConstants.Head0);
        _visualizer.SecondSide.SetImage(image, DiskHeadConstants.Head1);
        _visualizer.Header.ConfigureRevolutions(image.Tracks.Count == 0
            ? 0
            : image.Tracks.Max(track => track.Revolutions.Count));
        _inspector.SetImage(image);
        _visualizer.FirstSide.Visibility = heads.Contains(DiskHeadConstants.Head0) ? Visibility.Visible : Visibility.Collapsed;
        _visualizer.SecondSide.Visibility = heads.Contains(DiskHeadConstants.Head1) ? Visibility.Visible : Visibility.Collapsed;
        Grid.SetColumn(_visualizer.FirstSide, MediaVisualizationLayoutConstants.FaceOneColumn);
        Grid.SetColumnSpan(
            _visualizer.FirstSide,
            heads.Count == MediaVisualizationLayoutConstants.SingleVisibleFaceCount
            && heads.Contains(DiskHeadConstants.Head0)
                ? MediaVisualizationLayoutConstants.SingleFaceColumnSpan
                : MediaVisualizationLayoutConstants.TwoFaceColumnSpan);
        Grid.SetColumn(
            _visualizer.SecondSide,
            heads.Count == MediaVisualizationLayoutConstants.SingleVisibleFaceCount
            && heads.Contains(DiskHeadConstants.Head1)
                ? MediaVisualizationLayoutConstants.FaceOneColumn
                : MediaVisualizationLayoutConstants.FaceTwoColumn);
        Grid.SetColumnSpan(
            _visualizer.SecondSide,
            heads.Count == MediaVisualizationLayoutConstants.SingleVisibleFaceCount
            && heads.Contains(DiskHeadConstants.Head1)
                ? MediaVisualizationLayoutConstants.SingleFaceColumnSpan
                : MediaVisualizationLayoutConstants.TwoFaceColumnSpan);
        await PrepareTracksAsync(cancellationToken);
    }

    private async Task PrepareTracksAsync(CancellationToken cancellationToken)
    {
        var image = Image;
        if (image is null) return;
        var heads = image.Tracks.Select(track => track.Head).Distinct().OrderBy(head => head).ToArray();
        var cylindersByHead = image.Tracks
            .GroupBy(track => track.Head)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<int>)group.OrderBy(track => track.Cylinder).Select(track => track.Cylinder).ToArray());
        _face0Progress.Configure(
            DiskHeadConstants.Head0,
            cylindersByHead.GetValueOrDefault(DiskHeadConstants.Head0) ?? [],
            _localize(DiskImageResourceKeys.VisualSide, [DiskHeadConstants.Head0]));
        _face1Progress.Configure(
            DiskHeadConstants.Head1,
            cylindersByHead.GetValueOrDefault(DiskHeadConstants.Head1) ?? [],
            _localize(DiskImageResourceKeys.VisualSide, [DiskHeadConstants.Head1]));
        _viewModel.ProgressVisibility = Visibility.Visible;
        _viewModel.GlobalProgressVisibility = Visibility.Collapsed;
        _viewModel.Face0ProgressVisibility = heads.Contains(DiskHeadConstants.Head0) ? Visibility.Visible : Visibility.Collapsed;
        _viewModel.Face1ProgressVisibility = heads.Contains(DiskHeadConstants.Head1) ? Visibility.Visible : Visibility.Collapsed;
        var total = Math.Max(1, image.Tracks.Count);
        _viewModel.ProgressText = _localize(DiskImageResourceKeys.VisualAnalysingTrack, [0, total]);
        _visualizer.Overview.Configure(cylindersByHead);
        var completedByHead = heads.ToDictionary(head => head, _ => 0);
        var presentationOrder = image.Tracks
            .OrderBy(track => track.Cylinder)
            .ThenBy(track => track.Head)
            .Select(track => (track.Head, track.Cylinder))
            .ToArray();
        var presentationQueue = Channel.CreateUnbounded<ScpTrackPreparation>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        var presentationTask = PresentPreparedTracksAsync();
        var preparations = heads.Select(head =>
        {
            var view = head == DiskHeadConstants.Head0 ? _visualizer.FirstSide : _visualizer.SecondSide;
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
                    var view = preparation.Head == DiskHeadConstants.Head0 ? _visualizer.FirstSide : _visualizer.SecondSide;
                    var strip = preparation.Head == DiskHeadConstants.Head0 ? _face0Progress : _face1Progress;
                    _trackPreparations[(preparation.Head, preparation.Cylinder)] = preparation;
                    _trackPresentationOrder.Add((preparation.Head, preparation.Cylinder));
                    completedByHead[preparation.Head]++;
                    var current = Math.Min(total, completedByHead.Values.Sum());
                    strip.SetState(preparation.Cylinder, TrackSegmentState.Success);
                    strip.ClearActive();
                    _visualizer.Overview.MarkPrepared(preparation);
                    _viewModel.ProgressText = _localize(DiskImageResourceKeys.VisualAnalysingTrack, [current, total]);
                    view.RevealPreparedTrack(preparation.Cylinder);
                }, DispatcherPriority.Background, cancellationToken);
            }
        }
    }

    private void ShowProgress(string text, double value, bool indeterminate)
    {
        _viewModel.ProgressText = string.Empty;
        _viewModel.ProgressValue = value;
        _viewModel.ProgressIndeterminate = indeterminate;
        _viewModel.ProgressVisibility = Visibility.Collapsed;
        _viewModel.GlobalProgressVisibility = Visibility.Collapsed;
        _viewModel.Face0ProgressVisibility = Visibility.Collapsed;
        _viewModel.Face1ProgressVisibility = Visibility.Collapsed;
        _viewModel.Face0ProgressValue = 0;
        _viewModel.Face1ProgressValue = 0;
        _face0Progress.Reset();
        _face1Progress.Reset();
        _visualizer.SetRecognitionProgress(true, text, DisplayName ?? string.Empty, value, indeterminate);
        _viewModel.OperationText = _localize(DiskImageResourceKeys.TabRead, []);
        _viewModel.OperationBrush = Brushes.SeaGreen;
    }

    private sealed class ImmediateProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }
}
