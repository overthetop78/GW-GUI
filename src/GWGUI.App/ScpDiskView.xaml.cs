using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using GWGUI.Scp;

namespace GWGUI.App;

public partial class ScpDiskView : UserControl
{
    private ScpImage? _image;
    private int _head;
    private float _zoom = 1;
    public event EventHandler<ScpTrack?>? TrackSelected;
    public event EventHandler<float>? ZoomChanged;
    public ScpTrack? SelectedTrack { get; private set; }
    public float Zoom => _zoom;

    public ScpDiskView() => InitializeComponent();
    public void SetImage(ScpImage? image, int head) { _image = image; _head = head; SelectedTrack = null; Canvas.InvalidateVisual(); }
    public void SetZoom(float zoom, bool notify = false) { _zoom = Math.Clamp(zoom, .65f, 4f); Canvas.InvalidateVisual(); if (notify) ZoomChanged?.Invoke(this, _zoom); }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas; canvas.Clear(new SKColor(7, 10, 14));
        var tracks = _image?.Tracks.Where(x => x.Head == _head).OrderBy(x => x.Cylinder).ToArray() ?? [];
        var center = new SKPoint(e.Info.Width / 2f, e.Info.Height / 2f); var outer = Math.Min(e.Info.Width, e.Info.Height) * .47f * _zoom; var inner = outer * .25f;
        using var disk = new SKPaint { Color = new SKColor(17, 61, 43), IsAntialias = true }; canvas.DrawCircle(center, outer, disk);
        using var hub = new SKPaint { Color = new SKColor(4, 6, 8), IsAntialias = true }; canvas.DrawCircle(center, inner, hub);
        if (tracks.Length == 0) { DrawCentered(canvas, center, $"Face {_head}\nAucune donnée", SKColors.White); return; }
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
            if (ReferenceEquals(track, SelectedTrack)) { using var selected = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true }; canvas.DrawCircle(center, radius, selected); }
        }
        DrawCentered(canvas, center, $"Face {_head}", new SKColor(210, 218, 228));
    }

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
        var position = e.GetPosition(Canvas); var centerX = Canvas.ActualWidth / 2; var centerY = Canvas.ActualHeight / 2; var distance = Math.Sqrt(Math.Pow(position.X - centerX, 2) + Math.Pow(position.Y - centerY, 2));
        var outer = Math.Min(Canvas.ActualWidth, Canvas.ActualHeight) * .47 * _zoom; var inner = outer * .25; if (distance < inner || distance > outer) return;
        var index = Math.Clamp((int)((outer - distance) / ((outer - inner) / tracks.Length)), 0, tracks.Length - 1); SelectedTrack = tracks[index]; Canvas.InvalidateVisual(); TrackSelected?.Invoke(this, SelectedTrack);
    }
}
