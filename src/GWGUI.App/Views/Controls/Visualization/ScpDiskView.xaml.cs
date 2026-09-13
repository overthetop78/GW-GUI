using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Contracts.Views.Visualization;
using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.App.Functions.Rendering.Scp;
using GWGUI.App.Interfaces.Rendering.Scp;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Rendering.Scp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using GWGUI.MediaEngine;

using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class ScpDiskView : UserControl, IMediaVisualizationView
{
    private ScpImage? _image;
    private int _head;
    private float _zoom = 1;
    private float _panX;
    private float _panY;
    private DiskMediaCategory _mediaCategory;
    private Point? _panDragLast;
    private readonly IScpRenderer _renderer;
    public event EventHandler<ScpTrack?>? TrackSelected;
    public event EventHandler<float>? ZoomChanged;
    public event Action<int, long>? ElementSelected;
    public ScpTrack? SelectedTrack { get; private set; }
    public int? SelectedRevolutionIndex { get; private set; }
    public float Zoom => _zoom;

    public ScpDiskView() : this(new SkiaScpRenderer()) { }
    internal ScpDiskView(IScpRenderer renderer) { _renderer = renderer; InitializeComponent(); UpdateLabels(); }
    public void SetImage(ScpImage? image, int head)
    {
        _image = image;
        _head = head;
        SelectedTrack = null;
        SelectedRevolutionIndex = null;
        _renderer.ClearCache();
        UpdateLabels();
        ResetView();
    }
    public async Task PrepareAsync(IProgress<ScpTrackPreparation>? progress = null, CancellationToken cancellationToken = default)
    {
        if (_image is null) return;
        await _renderer.PrepareAsync(_image, _head, progress, cancellationToken);
        if (!cancellationToken.IsCancellationRequested) Canvas.InvalidateVisual();
    }
    public void SetDecoder(string? decoderId) { _renderer.DecoderId = decoderId; Canvas.InvalidateVisual(); }
    public void SetRevolution(int? revolutionIndex) { SelectedRevolutionIndex = revolutionIndex; Canvas.InvalidateVisual(); }
    public void SetMediaCategory(DiskMediaCategory mediaKind) { _mediaCategory = mediaKind; Canvas.InvalidateVisual(); }
    public void RevealPreparedTrack(int cylinder) { _renderer.RevealTrack(cylinder); Canvas.InvalidateVisual(); }
    public void RefreshPreparedTracks() => Canvas.InvalidateVisual();
    public void SetZoom(float zoom, bool notify = false) { _zoom = Math.Clamp(zoom, .65f, 4f); if (_zoom <= 1) _panX = _panY = 0; ResetZoomButton.Content = $"{_zoom:P0}"; Canvas.InvalidateVisual(); if (notify) ZoomChanged?.Invoke(this, _zoom); }
    public void ResetView() { _panX = _panY = 0; SetZoom(1); }

    public void SelectElement(int surface, long position)
    {
        if (surface != _head || position is < int.MinValue or > int.MaxValue) return;
        SelectTrack(_image?.Tracks.FirstOrDefault(track => track.Head == surface && track.Cylinder == (int)position), false);
    }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        _renderer.Render(e.Surface.Canvas, CreateRenderRequest(e.Info.Width, e.Info.Height));
    }

    internal ScpRenderRequest CreateRenderRequest(int width, int height)
    {
        var center = new SKPoint(width / 2f + _panX * width / (float)Math.Max(1, Canvas.ActualWidth), height / 2f + _panY * height / (float)Math.Max(1, Canvas.ActualHeight));
        return new(_image, _head, SelectedTrack, SelectedRevolutionIndex, width, height, center, _zoom,
            LocExtension.Get("Visual.SideNoData", _head), LocExtension.Get("Visual.Side", _head), _mediaCategory);
    }

    internal void PanBy(double x, double y)
    {
        if (_zoom <= 1) return;
        var maxX = Math.Max(0, Canvas.ActualWidth * (_zoom - 1) / 2);
        var maxY = Math.Max(0, Canvas.ActualHeight * (_zoom - 1) / 2);
        _panX = (float)Math.Clamp(_panX + x, -maxX, maxX);
        _panY = (float)Math.Clamp(_panY + y, -maxY, maxY);
        Canvas.InvalidateVisual();
    }

    private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e) { SetZoom(_zoom * (e.Delta > 0 ? 1.12f : .89f), true); e.Handled = true; }
    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => SelectTrackAt(e.GetPosition(Canvas));

    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || _zoom <= 1) return;
        _panDragLast = e.GetPosition(Canvas);
        Canvas.Cursor = VisualizationCursors.Grabbing;
        Canvas.CaptureMouse();
        e.Handled = true;
    }

    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || _panDragLast is null) return;
        _panDragLast = null;
        Canvas.ReleaseMouseCapture();
        Canvas.ClearValue(CursorProperty);
        e.Handled = true;
    }

    internal void SelectTrackAt(Point position)
    {
        var tracks = _image?.Tracks.Where(x => x.Head == _head).OrderBy(x => x.Cylinder).ToArray() ?? []; if (tracks.Length == 0) return;
        var centerX = Canvas.ActualWidth / 2 + _panX; var centerY = Canvas.ActualHeight / 2 + _panY; var distance = Math.Sqrt(Math.Pow(position.X - centerX, 2) + Math.Pow(position.Y - centerY, 2));
        var outer = ScpMediaGeometryFunctions.FluxRadius((int)Canvas.ActualWidth, (int)Canvas.ActualHeight, _zoom, _mediaCategory); var inner = outer * .25; if (distance < inner || distance > outer) return;
        var index = Math.Clamp((int)((outer - distance) / ((outer - inner) / tracks.Length)), 0, tracks.Length - 1);
        SelectTrack(tracks[index], true);
    }

    private void SelectTrack(ScpTrack? track, bool notifyOverview)
    {
        SelectedTrack = track;
        UpdateLabels();
        Canvas.InvalidateVisual();
        TrackSelected?.Invoke(this, track);
        if (notifyOverview && track is not null) ElementSelected?.Invoke(track.Head, track.Cylinder);
    }

    private void UpdateLabels()
    {
        SurfaceLabel.Text = LocExtension.Get("Visual.Side", _head);
        SelectionLabel.Text = SelectedTrack is null
            ? $"{LocExtension.Get("Visual.TrackLabel")} —"
            : $"{LocExtension.Get("Visual.TrackLabel")} {SelectedTrack.Cylinder}";
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoom * .89f, true);
    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoom * 1.12f, true);
    private void ResetZoomButton_Click(object sender, RoutedEventArgs e) => ResetView();

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(Canvas);
        if (_panDragLast is Point last && e.MiddleButton == MouseButtonState.Pressed)
        {
            PanBy(position.X - last.X, position.Y - last.Y);
            _panDragLast = position;
            return;
        }
    }
}
