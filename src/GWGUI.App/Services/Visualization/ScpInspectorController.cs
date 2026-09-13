using GWGUI.App.Contracts.Visualization;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Presenters.Visualization;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Views.Controls.Visualization;
using System.Windows;
using System.Windows.Controls;

using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Decoding;


using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.App.Services.Visualization;

/// <summary>
/// Coordinates track selection, linked zoom and the integrated SCP inspector.
/// Disk loading and progressive track preparation remain outside this controller.
/// </summary>
public sealed class ScpInspectorController : IDisposable
{
    private readonly VisualizerTabSection _section;
    private readonly DiskImageCancellationScope _cancellation;
    private readonly Func<CancellationToken, Task> _prepareViewsAsync;
    private readonly Action _hideProgress;
    private readonly ScpInspectorPresenter _presenter;
    private ScpImage? _image;
    private readonly Dictionary<int, ScpTrack> _selectedTracks = [];
    private bool _syncingZoom;
    private bool _disposed;

    public ScpInspectorController(
        Window owner,
        VisualizerTabSection section,
        FluxDecoderRegistry decoders,
        DiskImageCancellationScope cancellation,
        Func<CancellationToken, Task> prepareViewsAsync,
        Action hideProgress,
        Func<string, object[], string> localize)
    {
        _ = owner;
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
    }

    public void SetImage(ScpImage image)
    {
        _cancellation.CancelInspector();
        _image = image;
        _selectedTracks.Clear();
        _section.ClearInspectorModels();
    }

    public void ClearImage()
    {
        _cancellation.CancelInspector();
        _image = null;
        _selectedTracks.Clear();
        _section.ClearInspectorModels();
    }

    public void RefreshInspector()
    {
        if (_selectedTracks.Count > 0 && _image is not null)
            _ = RefreshInspectorsAsync();
    }

    private void TrackSelected(object? sender, ScpTrack? track)
    {
        var surface = ReferenceEquals(sender, _section.SecondSide) ? 1 : 0;
        _ = SelectTrackAsync(surface, track);
    }

    internal Task SelectTrackAsync(ScpTrack? track) => SelectTrackAsync(track?.Head ?? 0, track);

    internal Task SelectTrackAsync(int surface, ScpTrack? track)
    {
        if (track is not null)
        {
            _selectedTracks[surface] = track;
            return UpdateInspectorAsync(surface, track);
        }
        _cancellation.CancelInspector();
        _selectedTracks.Remove(surface);
        _section.SetInspectorModel(surface, null);
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

    private async Task UpdateInspectorAsync(int surface, ScpTrack track)
    {
        var image = _image;
        if (image is null) return;
        var cancellation = _cancellation.BeginInspector();
        var decoderId = (_section.Header.DecoderCombo.SelectedItem as ScpDecoderChoice)?.Id;
        try
        {
            var model = await Task.Run(() => _presenter.BuildCommonModel(image, track, decoderId), cancellation.Token);
            if (cancellation.IsCancellationRequested || !_cancellation.IsCurrentInspector(cancellation)) return;
            _section.SetInspectorModel(surface, model);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
    }

    private async Task RefreshInspectorsAsync()
    {
        var image = _image;
        if (image is null) return;
        var tracks = _selectedTracks.ToArray();
        var cancellation = _cancellation.BeginInspector();
        var decoderId = (_section.Header.DecoderCombo.SelectedItem as ScpDecoderChoice)?.Id;
        try
        {
            var models = await Task.Run(() => tracks.Select(item =>
                (item.Key, Model: _presenter.BuildCommonModel(image, item.Value, decoderId))).ToArray(), cancellation.Token);
            if (cancellation.IsCancellationRequested || !_cancellation.IsCurrentInspector(cancellation)) return;
            foreach (var item in models) _section.SetInspectorModel(item.Key, item.Model);
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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cancellation.CancelInspector();
        _section.FirstSide.TrackSelected -= TrackSelected;
        _section.SecondSide.TrackSelected -= TrackSelected;
        _section.FirstSide.ZoomChanged -= ZoomChanged;
        _section.SecondSide.ZoomChanged -= ZoomChanged;
        _section.Header.DecoderCombo.SelectionChanged -= DecoderChanged;
        _section.Header.ResetButton.Click -= ResetViews;
        _image = null;
        _selectedTracks.Clear();
        _section.ClearInspectorModels();
    }
}
