using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using GWGUI.Scp;
using GWGUI.Scp.Decoding;
using GWGUI.App.Localization;

namespace GWGUI.App;

public partial class ScpDiskView : UserControl
{
    private ScpImage? _image;
    private int _head;
    private float _zoom = 1;
    private float _panX;
    private float _panY;
    private Point? _dragOrigin;
    private readonly FluxDecoderRegistry _decoders = new();
    private readonly Dictionary<ScpTrack, FluxDecodeResult> _decodeCache = [];
    private string? _decoderId;
    public event EventHandler<ScpTrack?>? TrackSelected;
    public event EventHandler<float>? ZoomChanged;
    public ScpTrack? SelectedTrack { get; private set; }
    public float Zoom => _zoom;

    public ScpDiskView() => InitializeComponent();
    public void SetImage(ScpImage? image, int head) { _image = image; _head = head; SelectedTrack = null; _decodeCache.Clear(); ResetView(); }
    public void SetDecoder(string? decoderId) { _decoderId = decoderId; _decodeCache.Clear(); Canvas.InvalidateVisual(); }
    public void SetZoom(float zoom, bool notify = false) { _zoom = Math.Clamp(zoom, .65f, 4f); Canvas.InvalidateVisual(); if (notify) ZoomChanged?.Invoke(this, _zoom); }
    public void ResetView() { _zoom = 1; _panX = _panY = 0; Canvas.InvalidateVisual(); }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas; canvas.Clear(new SKColor(7, 10, 14));
        var tracks = _image?.Tracks.Where(x => x.Head == _head).OrderBy(x => x.Cylinder).ToArray() ?? [];
        var center = new SKPoint(e.Info.Width / 2f + _panX * e.Info.Width / (float)Math.Max(1, Canvas.ActualWidth), e.Info.Height / 2f + _panY * e.Info.Height / (float)Math.Max(1, Canvas.ActualHeight)); var outer = Math.Min(e.Info.Width, e.Info.Height) * .47f * _zoom; var inner = outer * .25f;
        using var disk = new SKPaint { Color = new SKColor(17, 61, 43), IsAntialias = true }; canvas.DrawCircle(center, outer, disk);
        using var hub = new SKPaint { Color = new SKColor(4, 6, 8), IsAntialias = true }; canvas.DrawCircle(center, inner, hub);
        if (tracks.Length == 0) { DrawCentered(canvas, center, LocExtension.Get("Visual.SideNoData", _head), SKColors.White); return; }
        var ring = (outer - inner) / Math.Max(1, tracks.Length);
        foreach (var track in tracks)
        {
            var trackIndex = Array.IndexOf(tracks, track); var radius = outer - ring * (trackIndex + .5f); var revolution = track.Revolutions.FirstOrDefault();
            if (revolution is null || revolution.FluxIntervals.Count == 0) continue;
            var sampleStep = Math.Max(1, revolution.FluxIntervals.Count / 1400); double total = revolution.FluxIntervals.Sum(x => (double)x); double elapsed = 0;
            var median = revolution.FluxIntervals.Order().ElementAt(revolution.FluxIntervals.Count / 2);
            for (var index = 0; index < revolution.FluxIntervals.Count; index += sampleStep)
            {
                double span = 0; for (var sample = index; sample < Math.Min(index + sampleStep, revolution.FluxIntervals.Count); sample++) span += revolution.FluxIntervals[sample];
                var start = (float)(elapsed / total * 360 - 90); var sweep = Math.Max(.08f, (float)(span / total * 360)); elapsed += span;
                var interval = revolution.FluxIntervals[index]; var color = interval < median * .65 ? new SKColor(143, 104, 255) : interval > median * 1.8 ? new SKColor(83, 173, 255) : new SKColor(36, 179, 93);
                using var paint = new SKPaint { Color = color, IsAntialias = false, Style = SKPaintStyle.Stroke, StrokeWidth = Math.Max(1, ring * .82f) };
                canvas.DrawArc(new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius), start, sweep, false, paint);
            }
            DrawDecodedStructures(canvas, center, radius, ring, track, revolution);
            if (ReferenceEquals(track, SelectedTrack)) { using var selected = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true }; canvas.DrawCircle(center, radius, selected); }
        }
        DrawCentered(canvas, center, $"Face {_head}", new SKColor(210, 218, 228));
    }

    private void DrawDecodedStructures(SKCanvas canvas, SKPoint center, float radius, float ring, ScpTrack track, ScpRevolution revolution)
    {
        if (!_decodeCache.TryGetValue(track, out var decoded))
        {
            decoded = _decoderId is null ? _decoders.DecodeAutomatic(revolution) : _decoders.Decode(_decoderId, revolution);
            _decodeCache[track] = decoded;
        }
        if (decoded.Structures.Count == 0 || decoded.EstimatedBitCellTicks <= 0) return;
        var totalBits = Math.Max(1d, revolution.FluxIntervals.Sum(x => (double)x) / decoded.EstimatedBitCellTicks);
        foreach (var structure in decoded.Structures)
        {
            var start = (float)(structure.BitOffset / totalBits * 360 - 90);
            var sweep = Math.Max(.18f, (float)(structure.BitLength / totalBits * 360));
            using var paint = new SKPaint { Color = StructureColor(structure.Kind), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = Math.Max(2, ring * .45f), StrokeCap = SKStrokeCap.Round };
            canvas.DrawArc(new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius), start, sweep, false, paint);
        }
    }

    private static SKColor StructureColor(FluxStructureKind kind) => kind switch
    {
        FluxStructureKind.IdAddressMark or FluxStructureKind.AppleAddress or FluxStructureKind.CommodoreHeader => new SKColor(255, 205, 64),
        FluxStructureKind.DataAddressMark or FluxStructureKind.AppleData => new SKColor(67, 220, 255),
        FluxStructureKind.DeletedDataAddressMark or FluxStructureKind.TimingAnomaly => new SKColor(255, 75, 96),
        _ => new SKColor(196, 117, 255)
    };

    private static void DrawCentered(SKCanvas canvas, SKPoint center, string text, SKColor color)
    {
        using var paint = new SKPaint { Color = color, IsAntialias = true };
        using var font = new SKFont(SKTypeface.Default, 17);
        var lines = text.Split('\n'); for (var index = 0; index < lines.Length; index++) canvas.DrawText(lines[index], center.X, center.Y + index * 20, SKTextAlign.Center, font, paint);
    }

    private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e) { SetZoom(_zoom * (e.Delta > 0 ? 1.12f : .89f), true); e.Handled = true; }
    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var tracks = _image?.Tracks.Where(x => x.Head == _head).OrderBy(x => x.Cylinder).ToArray() ?? []; if (tracks.Length == 0) return;
        var position = e.GetPosition(Canvas); var centerX = Canvas.ActualWidth / 2 + _panX; var centerY = Canvas.ActualHeight / 2 + _panY; var distance = Math.Sqrt(Math.Pow(position.X - centerX, 2) + Math.Pow(position.Y - centerY, 2));
        var outer = Math.Min(Canvas.ActualWidth, Canvas.ActualHeight) * .47 * _zoom; var inner = outer * .25; if (distance < inner || distance > outer) return;
        var index = Math.Clamp((int)((outer - distance) / ((outer - inner) / tracks.Length)), 0, tracks.Length - 1); SelectedTrack = tracks[index]; Canvas.InvalidateVisual(); TrackSelected?.Invoke(this, SelectedTrack);
    }

    private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e) { _dragOrigin = e.GetPosition(Canvas); Canvas.CaptureMouse(); e.Handled = true; }
    private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e) { _dragOrigin = null; Canvas.ReleaseMouseCapture(); e.Handled = true; }
    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(Canvas);
        if (_dragOrigin is Point origin && e.RightButton == MouseButtonState.Pressed) { _panX += (float)(position.X - origin.X); _panY += (float)(position.Y - origin.Y); _dragOrigin = position; Canvas.InvalidateVisual(); return; }
        var track = TrackAt(position); Canvas.ToolTip = track is null ? null : LocExtension.Get("Visual.TrackTooltip", track.Head, track.Cylinder, track.Revolutions.Count);
    }
    private void Canvas_MouseLeave(object sender, MouseEventArgs e) { if (_dragOrigin is null) Canvas.ToolTip = null; }
    private ScpTrack? TrackAt(Point position)
    {
        var tracks = _image?.Tracks.Where(x => x.Head == _head).OrderBy(x => x.Cylinder).ToArray() ?? []; if (tracks.Length == 0) return null;
        var outer = Math.Min(Canvas.ActualWidth, Canvas.ActualHeight) * .47 * _zoom; var inner = outer * .25; var distance = Math.Sqrt(Math.Pow(position.X - (Canvas.ActualWidth / 2 + _panX), 2) + Math.Pow(position.Y - (Canvas.ActualHeight / 2 + _panY), 2));
        if (distance < inner || distance > outer) return null; return tracks[Math.Clamp((int)((outer - distance) / ((outer - inner) / tracks.Length)), 0, tracks.Length - 1)];
    }
}
