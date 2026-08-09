using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using GWGUI.Scp;
using GWGUI.App.Localization;
using GWGUI.App.Rendering;

namespace GWGUI.App;

public partial class ScpDiskView : UserControl
{
    private ScpImage? _image;
    private int _head;
    private float _zoom = 1;
    private float _panX;
    private float _panY;
    private DiskMediaKind _mediaKind;
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
    public void SetMediaKind(DiskMediaKind mediaKind) { _mediaKind = mediaKind; Canvas.InvalidateVisual(); }
    public void RefreshPreparedTracks() => Canvas.InvalidateVisual();
    public void SetZoom(float zoom, bool notify = false) { _zoom = Math.Clamp(zoom, .65f, 4f); Canvas.InvalidateVisual(); if (notify) ZoomChanged?.Invoke(this, _zoom); }
    public void ResetView() { _zoom = 1; _panX = _panY = 0; Canvas.InvalidateVisual(); }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var center = new SKPoint(e.Info.Width / 2f + _panX * e.Info.Width / (float)Math.Max(1, Canvas.ActualWidth), e.Info.Height / 2f + _panY * e.Info.Height / (float)Math.Max(1, Canvas.ActualHeight));
        _renderer.Render(e.Surface.Canvas, new ScpRenderRequest(_image, _head, SelectedTrack, e.Info.Width, e.Info.Height, center, _zoom,
            LocExtension.Get("Visual.SideNoData", _head), LocExtension.Get("Visual.Side", _head), _mediaKind));
    }

    private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e) { SetZoom(_zoom * (e.Delta > 0 ? 1.12f : .89f), true); e.Handled = true; }
    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var tracks = _image?.Tracks.Where(x => x.Head == _head).OrderBy(x => x.Cylinder).ToArray() ?? []; if (tracks.Length == 0) return;
        var position = e.GetPosition(Canvas); var centerX = Canvas.ActualWidth / 2 + _panX; var centerY = Canvas.ActualHeight / 2 + _panY; var distance = Math.Sqrt(Math.Pow(position.X - centerX, 2) + Math.Pow(position.Y - centerY, 2));
        var outer = ScpMediaGeometry.FluxRadius((int)Canvas.ActualWidth, (int)Canvas.ActualHeight, _zoom, _mediaKind); var inner = outer * .25; if (distance < inner || distance > outer) return;
        var index = Math.Clamp((int)((outer - distance) / ((outer - inner) / tracks.Length)), 0, tracks.Length - 1); SelectedTrack = tracks[index]; Canvas.InvalidateVisual(); TrackSelected?.Invoke(this, SelectedTrack);
    }

    private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e) { _dragOrigin = e.GetPosition(Canvas); Canvas.CaptureMouse(); e.Handled = true; }
    private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e) { _dragOrigin = null; Canvas.ReleaseMouseCapture(); e.Handled = true; }
    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(Canvas);
        if (_dragOrigin is Point origin && e.RightButton == MouseButtonState.Pressed) { _panX += (float)(position.X - origin.X); _panY += (float)(position.Y - origin.Y); _dragOrigin = position; Canvas.InvalidateVisual(); return; }
    }
}
