using GWGUI.App.Contracts.Rendering.Scp;
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
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class ScpDiskView : UserControl
{
    private ScpImage? _image;
    private int _head;
    private float _zoom = 1;
    private float _panX;
    private float _panY;
    private DiskMediaCategory _mediaCategory;
    private Point? _dragOrigin;
    private readonly IScpRenderer _renderer;
    public event EventHandler<ScpTrack?>? TrackSelected;
    public event EventHandler<float>? ZoomChanged;
    public ScpTrack? SelectedTrack { get; private set; }
    public float Zoom => _zoom;

    public ScpDiskView() : this(new SkiaScpRenderer()) { }
    internal ScpDiskView(IScpRenderer renderer) { _renderer = renderer; InitializeComponent(); }
    public void SetImage(ScpImage? image, int head) { _image = image; _head = head; SelectedTrack = null; _renderer.ClearCache(); ResetView(); }
    public async Task PrepareAsync(IProgress<ScpTrackPreparation>? progress = null, CancellationToken cancellationToken = default)
    {
        if (_image is null) return;
        await _renderer.PrepareAsync(_image, _head, progress, cancellationToken);
        if (!cancellationToken.IsCancellationRequested) Canvas.InvalidateVisual();
    }
    public void SetDecoder(string? decoderId) { _renderer.DecoderId = decoderId; Canvas.InvalidateVisual(); }
    public void SetMediaCategory(DiskMediaCategory mediaKind) { _mediaCategory = mediaKind; Canvas.InvalidateVisual(); }
    public void RefreshPreparedTracks() => Canvas.InvalidateVisual();
    public void SetZoom(float zoom, bool notify = false) { _zoom = Math.Clamp(zoom, .65f, 4f); Canvas.InvalidateVisual(); if (notify) ZoomChanged?.Invoke(this, _zoom); }
    public void ResetView() { _zoom = 1; _panX = _panY = 0; Canvas.InvalidateVisual(); }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        _renderer.Render(e.Surface.Canvas, CreateRenderRequest(e.Info.Width, e.Info.Height));
    }

    internal ScpRenderRequest CreateRenderRequest(int width, int height)
    {
        var center = new SKPoint(width / 2f + _panX * width / (float)Math.Max(1, Canvas.ActualWidth), height / 2f + _panY * height / (float)Math.Max(1, Canvas.ActualHeight));
        return new(_image, _head, SelectedTrack, width, height, center, _zoom,
            LocExtension.Get("Visual.SideNoData", _head), LocExtension.Get("Visual.Side", _head), _mediaCategory);
    }

    internal void PanBy(double x, double y)
    {
        _panX += (float)x; _panY += (float)y; Canvas.InvalidateVisual();
    }

    private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e) { SetZoom(_zoom * (e.Delta > 0 ? 1.12f : .89f), true); e.Handled = true; }
    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => SelectTrackAt(e.GetPosition(Canvas));

    internal void SelectTrackAt(Point position)
    {
        var tracks = _image?.Tracks.Where(x => x.Head == _head).OrderBy(x => x.Cylinder).ToArray() ?? []; if (tracks.Length == 0) return;
        var centerX = Canvas.ActualWidth / 2 + _panX; var centerY = Canvas.ActualHeight / 2 + _panY; var distance = Math.Sqrt(Math.Pow(position.X - centerX, 2) + Math.Pow(position.Y - centerY, 2));
        var outer = ScpMediaGeometryFunctions.FluxRadius((int)Canvas.ActualWidth, (int)Canvas.ActualHeight, _zoom, _mediaCategory); var inner = outer * .25; if (distance < inner || distance > outer) return;
        var index = Math.Clamp((int)((outer - distance) / ((outer - inner) / tracks.Length)), 0, tracks.Length - 1); SelectedTrack = tracks[index]; Canvas.InvalidateVisual(); TrackSelected?.Invoke(this, SelectedTrack);
    }

    private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e) { _dragOrigin = e.GetPosition(Canvas); Canvas.CaptureMouse(); e.Handled = true; }
    private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e) { _dragOrigin = null; Canvas.ReleaseMouseCapture(); e.Handled = true; }
    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(Canvas);
        if (_dragOrigin is Point origin && e.RightButton == MouseButtonState.Pressed) { PanBy(position.X - origin.X, position.Y - origin.Y); _dragOrigin = position; return; }
    }
}
