using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Controls;
using GWGUI.App.ViewModels;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Decoding;

namespace GWGUI.App.Services;

/// <summary>
/// Coordinates track selection, linked zoom and the attached/detached SCP inspector.
/// Disk loading and progressive track preparation remain outside this controller.
/// </summary>
public sealed class ScpInspectorController
{
    private readonly Window _owner;
    private readonly VisualizerTabSection _section;
    private readonly DiskImageCancellationScope _cancellation;
    private readonly Func<CancellationToken, Task> _prepareViewsAsync;
    private readonly Action _hideProgress;
    private readonly ScpInspectorPresenter _presenter;
    private ScpImage? _image;
    private ScpTrack? _selectedTrack;
    private ScpInspectorWindow? _detachedWindow;
    private bool _syncingZoom;

    public ScpInspectorController(
        Window owner,
        VisualizerTabSection section,
        FluxDecoderRegistry decoders,
        DiskImageCancellationScope cancellation,
        Func<CancellationToken, Task> prepareViewsAsync,
        Action hideProgress,
        Func<string, object[], string> localize)
    {
        _owner = owner;
        _section = section;
        _cancellation = cancellation;
        _prepareViewsAsync = prepareViewsAsync;
        _hideProgress = hideProgress;
        _presenter = new ScpInspectorPresenter(decoders, localize);

        _section.FirstSide.TrackSelected += TrackSelected;
        _section.SecondSide.TrackSelected += TrackSelected;
        _section.FirstSide.ZoomChanged += ZoomChanged;
        _section.SecondSide.ZoomChanged += ZoomChanged;
        _section.Header.DecoderCombo.SelectionChanged += DecoderChanged;
        _section.Header.ResetButton.Click += ResetViews;
        _section.Inspector.CloseRequested += (_, _) => _section.Inspector.Visibility = Visibility.Collapsed;
        _section.Inspector.DetachRequested += (_, _) => DetachInspector();
        _section.Inspector.DragRequested += (_, delta) => MoveInspector(delta.X, delta.Y);
        _section.ToggleInspectorRequested += ToggleInspector;
    }

    public void SetImage(ScpImage image)
    {
        _image = image;
        _selectedTrack = null;
        _section.Inspector.DataContext = null;
        if (_detachedWindow is not null) _detachedWindow.DataContext = null;
    }

    public void ClearImage()
    {
        _image = null;
        _selectedTrack = null;
        _section.Inspector.DataContext = null;
        if (_detachedWindow is not null) _detachedWindow.DataContext = null;
    }

    public void RefreshInspector()
    {
        if (_selectedTrack is not null && _image is not null)
            _ = UpdateInspectorAsync(_selectedTrack);
    }

    private void TrackSelected(object? sender, ScpTrack? track)
    {
        _selectedTrack = track;
        RefreshInspector();
    }

    private async void DecoderChanged(object sender, SelectionChangedEventArgs e)
    {
        var decoderId = (_section.Header.DecoderCombo.SelectedItem as ScpDecoderChoice)?.Id;
        _section.FirstSide.SetDecoder(decoderId);
        _section.SecondSide.SetDecoder(decoderId);
        if (_image is null)
        {
            RefreshInspector();
            return;
        }

        var cancellation = _cancellation.BeginScp();
        try
        {
            await _prepareViewsAsync(cancellation.Token);
            RefreshInspector();
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        finally
        {
            if (_cancellation.IsCurrentScp(cancellation)) _hideProgress();
        }
    }

    private async Task UpdateInspectorAsync(ScpTrack track)
    {
        var image = _image;
        if (image is null) return;
        var cancellation = _cancellation.BeginInspector();
        var decoderId = (_section.Header.DecoderCombo.SelectedItem as ScpDecoderChoice)?.Id;
        try
        {
            var model = await Task.Run(() => _presenter.BuildModel(image, track, decoderId), cancellation.Token);
            if (cancellation.IsCancellationRequested || !_cancellation.IsCurrentInspector(cancellation)) return;
            _section.Inspector.DataContext = model;
            _section.Inspector.Visibility = _detachedWindow is null ? Visibility.Visible : Visibility.Collapsed;
            if (_detachedWindow is null) PositionInspector();
            else _detachedWindow.DataContext = model;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
    }

    private void ZoomChanged(object? sender, float zoom)
    {
        if (_syncingZoom || _section.Header.LinkZoomCheckBox.IsChecked != true) return;
        _syncingZoom = true;
        try
        {
            (ReferenceEquals(sender, _section.FirstSide) ? _section.SecondSide : _section.FirstSide).SetZoom(zoom);
        }
        finally
        {
            _syncingZoom = false;
        }
    }

    private void ResetViews(object sender, RoutedEventArgs e)
    {
        _section.FirstSide.ResetView();
        _section.SecondSide.ResetView();
    }

    private void ToggleInspector(object sender, RoutedEventArgs e)
    {
        if (_detachedWindow is not null)
        {
            _detachedWindow.Activate();
            return;
        }

        _section.Inspector.Visibility = _section.Inspector.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void MoveInspector(double x, double y)
    {
        var inspector = _section.Inspector;
        var layer = _section.InspectorCanvas;
        var left = Math.Clamp(Canvas.GetLeft(inspector) + x, 0, Math.Max(0, layer.ActualWidth - inspector.ActualWidth));
        var top = Math.Clamp(Canvas.GetTop(inspector) + y, 0, Math.Max(0, layer.ActualHeight - inspector.ActualHeight));
        Canvas.SetLeft(inspector, left);
        Canvas.SetTop(inspector, top);
    }

    private void PositionInspector()
    {
        var inspector = _section.Inspector;
        var layer = _section.InspectorCanvas;
        inspector.Width = Math.Max(320, Math.Min(390, layer.ActualWidth - 12));
        inspector.Height = Math.Max(280, Math.Min(410, layer.ActualHeight - 12));
        var currentLeft = Canvas.GetLeft(inspector);
        var currentTop = Canvas.GetTop(inspector);
        var left = double.IsNaN(currentLeft) ? Math.Max(12, layer.ActualWidth - inspector.Width - 20) : Math.Min(currentLeft, Math.Max(0, layer.ActualWidth - inspector.Width));
        var top = double.IsNaN(currentTop) ? 18 : Math.Min(currentTop, Math.Max(0, layer.ActualHeight - inspector.Height));
        Canvas.SetLeft(inspector, left);
        Canvas.SetTop(inspector, top);
    }

    private void DetachInspector()
    {
        if (_detachedWindow is not null) return;
        _section.Inspector.Visibility = Visibility.Collapsed;
        var window = _detachedWindow = new ScpInspectorWindow { Owner = _owner, DataContext = _section.Inspector.DataContext };
        window.AttachRequested += (_, _) => _section.Inspector.Visibility = Visibility.Visible;
        window.Closed += (_, _) => _detachedWindow = null;
        window.Show();
    }
}
