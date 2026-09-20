using GWGUI.App.Contracts.Rendering.Blocks;
using GWGUI.App.Contracts.Progress;
using GWGUI.App.Contracts.Rendering.Optical;
using GWGUI.App.Contracts.Rendering.Sectors;
using GWGUI.App.Contracts.Rendering.Sequential;
using GWGUI.App.Constants.DiskImages;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Presenters.Visualization;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Representations.Blocks;
using GWGUI.MediaEngine.Representations.Optical;
using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaEngine.Representations.Sequential;
using GWGUI.MediaEngine.Visualization;
using System.Windows;
using System.Windows.Threading;

namespace GWGUI.App.Services.DiskImages.Visualization;

internal sealed class MediaVisualizationController
{
    private static readonly TimeSpan MinimumSectorTrackPresentationInterval = TimeSpan.FromMilliseconds(100);

    private readonly VisualizerTabSection _visualizer;
    private readonly MainWindowViewModel _viewModel;
    private readonly TrackProgressStrip _face0Progress;
    private readonly TrackProgressStrip _face1Progress;
    private readonly MediaVisualizationProviderRegistry? _visualizationProviders;
    private readonly Func<string, object[], string> _localize;
    private readonly SectorMediaView _sectorView;
    private readonly SectorMediaInspectorPresenter _sectorPresenter;
    private readonly BlockMediaView _blockView;
    private readonly BlockMediaInspectorPresenter _blockPresenter;
    private readonly OpticalMediaView _opticalView;
    private readonly OpticalMediaInspectorPresenter _opticalPresenter;
    private readonly SequentialMediaView _sequentialView;
    private readonly SequentialMediaInspectorPresenter _sequentialPresenter;
    private BlockMediaRenderModel? _blockRenderModel;
    private BlockMediaRange? _selectedBlockRange;
    private int? _selectedBlockSurface;
    private MediaImageDocument? _opticalDocument;
    private OpticalMediaRenderModel? _opticalRenderModel;

    internal MediaVisualizationController(
        VisualizerTabSection visualizer,
        MainWindowViewModel viewModel,
        TrackProgressStrip face0Progress,
        TrackProgressStrip face1Progress,
        MediaVisualizationProviderRegistry? visualizationProviders,
        Func<string, object[], string> localize)
    {
        _visualizer = visualizer;
        _viewModel = viewModel;
        _face0Progress = face0Progress;
        _face1Progress = face1Progress;
        _visualizationProviders = visualizationProviders;
        _localize = localize;

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
    }

    internal int SectorRevealedTrackCount => _sectorView.RevealedTrackCount;

    internal SequentialMediaRenderModel? SequentialRenderModel { get; private set; }

    internal async Task<bool> PresentAsync(MediaImageDocument document, CancellationToken cancellationToken)
    {
        var descriptor = _visualizationProviders?.CreateDescriptor(document);
        if (descriptor is null) return false;

        if (document.Representation is SectorMediaImageRepresentation sectorRepresentation)
        {
            await PresentSectorImageAsync(document, descriptor, sectorRepresentation.Image, cancellationToken, true);
        }
        else
        {
            if (document.Representation is BlockMediaImageRepresentation)
            {
                _blockRenderModel = _blockPresenter.BuildRenderModel(document);
                _selectedBlockRange = null;
                _selectedBlockSurface = null;
                _blockView.SetDocument(_blockRenderModel);
            }
            else if (document.Representation is OpticalMediaImageRepresentation)
            {
                _opticalDocument = document;
                _opticalRenderModel = _opticalPresenter.BuildRenderModel(document);
                _opticalView.SetDocument(_opticalRenderModel);
            }
            else if (document.Representation is SequentialMediaImageRepresentation)
            {
                SequentialRenderModel = _sequentialPresenter.BuildRenderModel(document);
                _sequentialView.SetDocument(SequentialRenderModel);
                ConfigureSequentialProgress(SequentialRenderModel);
            }

            _visualizer.ShowDocument(document, descriptor);
            if (document.Representation is SequentialMediaImageRepresentation && SequentialRenderModel is not null)
            {
                _visualizer.Overview.MarkSequential(SequentialRenderModel);
                _visualizer.SetInspectorModel(_sequentialPresenter.BuildInspectorModel(SequentialRenderModel, null));
            }
        }

        return descriptor.RepresentationKind != MediaRepresentationKind.Flux;
    }

    internal async Task PresentSectorImageAsync(
        MediaImageDocument document,
        MediaVisualizationDescriptor descriptor,
        SectorImage image,
        CancellationToken cancellationToken,
        bool showProgress = false)
    {
        var geometry = _sectorPresenter.BuildGeometryModel(image);
        if (showProgress)
        {
            var cylindersBySurface = geometry.Surfaces.ToDictionary(
                surface => surface.Index,
                surface => (IReadOnlyList<int>)surface.Tracks.Select(track => track.Cylinder).ToArray());
            _face0Progress.Configure(
                MediaVisualizationLayoutConstants.FaceOne,
                cylindersBySurface.GetValueOrDefault(MediaVisualizationLayoutConstants.FaceOne) ?? [],
                _localize(DiskImageResourceKeys.VisualSide, [MediaVisualizationLayoutConstants.FaceOne]));
            _face1Progress.Configure(
                MediaVisualizationLayoutConstants.FaceTwo,
                cylindersBySurface.GetValueOrDefault(MediaVisualizationLayoutConstants.FaceTwo) ?? [],
                _localize(DiskImageResourceKeys.VisualSide, [MediaVisualizationLayoutConstants.FaceTwo]));
            _viewModel.ProgressVisibility = Visibility.Visible;
            _viewModel.GlobalProgressVisibility = Visibility.Collapsed;
            _viewModel.Face0ProgressVisibility = cylindersBySurface.ContainsKey(MediaVisualizationLayoutConstants.FaceOne) ? Visibility.Visible : Visibility.Collapsed;
            _viewModel.Face1ProgressVisibility = cylindersBySurface.ContainsKey(MediaVisualizationLayoutConstants.FaceTwo) ? Visibility.Visible : Visibility.Collapsed;
            _viewModel.ProgressIndeterminate = false;
            _viewModel.ProgressText = string.Empty;
        }

        _sectorView.SetDocument(geometry, descriptor);
        _visualizer.ShowDocument(document, descriptor);
        var lastPresentation = System.Diagnostics.Stopwatch.GetTimestamp();
        foreach (var position in geometry.Surfaces
                     .SelectMany(surface => surface.Tracks.Select(track => (Surface: surface.Index, track.Cylinder)))
                     .OrderBy(item => item.Cylinder)
                     .ThenBy(item => item.Surface))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var track = _sectorPresenter.AnalyzeTrack(image, position.Surface, position.Cylinder);
            var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(lastPresentation);
            var remaining = RemainingSectorTrackPresentationDelay(elapsed);
            if (remaining > TimeSpan.Zero) await Task.Delay(remaining, cancellationToken);
            await _visualizer.Dispatcher.InvokeAsync(() =>
            {
                _sectorView.RevealTrack(position.Surface, track);
                _visualizer.Overview.MarkSectorTrack(position.Surface, track);
                if (showProgress)
                {
                    var progress = position.Surface == MediaVisualizationLayoutConstants.FaceOne
                        ? _face0Progress
                        : _face1Progress;
                    progress.SetState(position.Cylinder, TrackSegmentState.Success);
                }
            }, DispatcherPriority.Background, cancellationToken);
            lastPresentation = System.Diagnostics.Stopwatch.GetTimestamp();
        }
    }

    internal void ConfigureSequentialProgress(SequentialMediaRenderModel model)
    {
        var rowCount = model.Segments.Count >= MediaVisualizationLayoutConstants.MinimumSegmentCountForSplitProgress
            ? MediaVisualizationLayoutConstants.SegmentProgressPageCount
            : MediaVisualizationLayoutConstants.SingleProgressPageCount;
        var rowSize = (model.Segments.Count + rowCount - 1) / rowCount;
        var firstCount = Math.Min(rowSize, model.Segments.Count);
        var secondCount = Math.Max(0, model.Segments.Count - firstCount);
        _face0Progress.Configure(
            MediaVisualizationProgressUnit.Segment,
            MediaVisualizationLayoutConstants.SegmentProgressPageOne,
            Enumerable.Range(0, firstCount).Select(index => (long)index).ToArray(),
            _localize(DiskImageResourceKeys.ExplorerCassette, []));
        if (secondCount > 0)
        {
            _face1Progress.Configure(
                MediaVisualizationProgressUnit.Segment,
                MediaVisualizationLayoutConstants.SegmentProgressPageTwo,
                Enumerable.Range(0, secondCount).Select(index => (long)index).ToArray(),
                _localize(DiskImageResourceKeys.ExplorerCassette, []));
        }

        _viewModel.ProgressVisibility = Visibility.Visible;
        _viewModel.GlobalProgressVisibility = Visibility.Collapsed;
        _viewModel.Face0ProgressVisibility = firstCount > 0 ? Visibility.Visible : Visibility.Collapsed;
        _viewModel.Face1ProgressVisibility = secondCount > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    internal static TimeSpan RemainingSectorTrackPresentationDelay(TimeSpan elapsedSincePresentation)
    {
        var remaining = MinimumSectorTrackPresentationInterval - elapsedSincePresentation;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    private void HandleSectorSelected(int surface, SectorMediaElement? sector) =>
        _visualizer.SetInspectorModel(surface, sector is null ? null : _sectorPresenter.BuildInspectorModel(surface, sector));

    private void HandleBlockRangeSelected(BlockMediaRange? range)
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

    private void HandleOpticalTrackSelected(OpticalMediaTrack? track) =>
        _visualizer.SetInspectorModel(_opticalDocument is null || _opticalRenderModel is null
            ? null
            : _opticalPresenter.BuildInspectorModel(_opticalDocument, _opticalRenderModel, track));

    private void HandleSequentialSegmentSelected(
        GWGUI.App.Contracts.Rendering.Sequential.SequentialMediaSegment? segment) =>
        _visualizer.SetInspectorModel(SequentialRenderModel is null
            ? null
            : _sequentialPresenter.BuildInspectorModel(SequentialRenderModel, segment));
}
