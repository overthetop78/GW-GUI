using GWGUI.App.Contracts.Visualization;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Presenters.Visualization;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.App.Views.Windows.Visualization;
using System.Windows;
using System.Windows.Controls;

using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Decoding;


using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.App.Services.Visualization;

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
    private MediaInspectorModel? _currentInspectorModel;
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
        _section.ToggleInspectorRequested += ToggleInspector;
        _section.DetachInspectorRequested += (_, _) => DetachInspector();
    }

    public void SetImage(ScpImage image)
    {
        _cancellation.CancelInspector();
        _image = image;
        _selectedTrack = null;
        _currentInspectorModel = null;
        _section.SetInspectorModel(null);
        if (_detachedWindow is not null) _detachedWindow.DataContext = null;
    }

    public void ClearImage()
    {
        _cancellation.CancelInspector();
        _image = null;
        _selectedTrack = null;
        _currentInspectorModel = null;
        _section.SetInspectorModel(null);
        if (_detachedWindow is not null) _detachedWindow.DataContext = null;
    }

    public void RefreshInspector()
    {
        if (_selectedTrack is not null && _image is not null)
            _ = UpdateInspectorAsync(_selectedTrack);
    }

    private void TrackSelected(object? sender, ScpTrack? track)
    {
        _ = SelectTrackAsync(track);
    }

    internal Task SelectTrackAsync(ScpTrack? track)
    {
        _selectedTrack = track;
        if (track is not null) return UpdateInspectorAsync(track);
        _cancellation.CancelInspector();
        _currentInspectorModel = null;
        _section.SetInspectorModel(null);
        if (_detachedWindow is not null) _detachedWindow.DataContext = null;
        return Task.CompletedTask;
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
            var model = await Task.Run(() => _presenter.BuildCommonModel(image, track, decoderId), cancellation.Token);
            if (cancellation.IsCancellationRequested || !_cancellation.IsCurrentInspector(cancellation)) return;
            _currentInspectorModel = model;
            if (_detachedWindow is null) _section.SetInspectorModel(model);
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

        _section.SetInspectorModel(_section.IsInspectorVisible
            ? null
            : _currentInspectorModel);
    }

    private void DetachInspector()
    {
        if (_detachedWindow is not null) return;
        _section.SetInspectorModel(null);
        var window = _detachedWindow = new ScpInspectorWindow { Owner = _owner, DataContext = _currentInspectorModel };
        window.AttachRequested += (_, _) => _section.SetInspectorModel(_currentInspectorModel);
        window.Closed += (_, _) => _detachedWindow = null;
        window.Show();
    }
}
