using GWGUI.App.Contracts.Rendering.Sectors;
using GWGUI.App.Contracts.Views.Visualization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Rendering.Sectors;
using GWGUI.MediaEngine.Images.Visualization;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class SectorMediaView : UserControl, IMediaVisualizationView
{
    private readonly SkiaSectorMediaRenderer _renderer = new();
    private readonly long?[] _selectedPositions = new long?[2];
    private readonly float[] _zooms = [1, 1];
    private readonly (int Width, int Height)[] _renderSizes = new (int Width, int Height)[2];
    private readonly Point[] _pan = [new(), new()];
    private Point? _panDragLast;
    private int _panDragSurface = -1;
    private readonly HashSet<(int Surface, int Cylinder)> _revealedTracks = [];
    private SectorMediaRenderModel? _model;
    private MediaVisualizationDescriptor? _descriptor;

    public SectorMediaView()
    {
        InitializeComponent();
        UpdateSurfaceLabels();
    }

    public event Action<int, long>? ElementSelected;
    public event Action<int, SectorMediaElement?>? SectorSelected;
    public bool LinkZoom { get; set; } = true;

    public void SetDocument(SectorMediaRenderModel model, MediaVisualizationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(descriptor);
        _model = model;
        _descriptor = descriptor;
        _revealedTracks.Clear();
        Array.Clear(_selectedPositions);
        _zooms[0] = _zooms[1] = 1;
        _pan[0] = _pan[1] = new Point();
        UpdateZoomLabel(0);
        UpdateZoomLabel(1);
        UpdateSelectionLabel(0, null);
        UpdateSelectionLabel(1, null);
        ConfigureSurfaceLayout(model.Surfaces.Select(surface => surface.Index).ToHashSet());
        Canvas0.InvalidateVisual();
        Canvas1.InvalidateVisual();
    }

    public void RevealTrack(int surface, SectorMediaTrack track)
    {
        if (_model is null) return;
        _model = _model with
        {
            Surfaces = _model.Surfaces.Select(item => item.Index != surface
                ? item
                : item with
                {
                    Tracks = item.Tracks.Select(existing => existing.Cylinder == track.Cylinder ? track : existing).ToArray()
                }).ToArray()
        };
        _revealedTracks.Add((surface, track.Cylinder));
        CanvasFor(surface).InvalidateVisual();
    }

    internal int GeometryTrackCount => _model?.Surfaces.Sum(surface => surface.Tracks.Count) ?? 0;
    internal int RevealedTrackCount => _revealedTracks.Count;

    public void SelectElement(int surface, long position)
    {
        if (surface is < 0 or > 1 || _model?.Surfaces.All(item => item.Index != surface) != false) return;
        var sector = _model.Surfaces.First(item => item.Index == surface).Tracks
            .FirstOrDefault(track => track.Cylinder == position)?.Sectors.FirstOrDefault();
        _selectedPositions[surface] = sector?.Position;
        UpdateSelectionLabel(surface, sector);
        CanvasFor(surface).InvalidateVisual();
        SectorSelected?.Invoke(surface, sector);
    }

    private void ConfigureSurfaceLayout(IReadOnlySet<int> surfaces)
    {
        var hasSide0 = surfaces.Contains(0);
        var hasSide1 = surfaces.Contains(1);
        Side0Panel.Visibility = hasSide0 ? Visibility.Visible : Visibility.Collapsed;
        Side1Panel.Visibility = hasSide1 ? Visibility.Visible : Visibility.Collapsed;
        Grid.SetColumn(Side0Panel, 0);
        Grid.SetColumnSpan(Side0Panel, hasSide0 && !hasSide1 ? 2 : 1);
        Grid.SetColumn(Side1Panel, hasSide1 && !hasSide0 ? 0 : 1);
        Grid.SetColumnSpan(Side1Panel, hasSide1 && !hasSide0 ? 2 : 1);
    }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var surface = SurfaceFor(sender);
        if (_descriptor is null || surface is < 0 or > 1)
        {
            e.Surface.Canvas.Clear(SKColors.Black);
            return;
        }

        _renderSizes[surface] = (e.Info.Width, e.Info.Height);

        var scaleX = e.Info.Width / Math.Max(1, CanvasFor(surface).ActualWidth);
        var scaleY = e.Info.Height / Math.Max(1, CanvasFor(surface).ActualHeight);
        e.Surface.Canvas.Save();
        e.Surface.Canvas.Translate((float)(_pan[surface].X * scaleX), (float)(_pan[surface].Y * scaleY));
        _renderer.Render(
            e.Surface.Canvas,
            _model,
            surface,
            _descriptor.Direction,
            _selectedPositions[surface],
            e.Info.Width,
            e.Info.Height,
            _zooms[surface],
            _revealedTracks);
        e.Surface.Canvas.Restore();
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_descriptor is null || sender is not SKElement canvas) return;
        var surface = SurfaceFor(canvas);
        if (surface is < 0 or > 1) return;
        SelectAt(canvas, surface, e.GetPosition(canvas));
    }

    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || sender is not SKElement canvas) return;
        var surface = SurfaceFor(canvas);
        if (surface is < 0 or > 1 || _zooms[surface] <= 1) return;
        _panDragSurface = surface;
        _panDragLast = e.GetPosition(canvas);
        canvas.Cursor = VisualizationCursors.Grabbing;
        canvas.CaptureMouse();
        e.Handled = true;
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not SKElement canvas || _panDragSurface != SurfaceFor(canvas)
            || _panDragLast is not Point last || e.MiddleButton != MouseButtonState.Pressed) return;
        var point = e.GetPosition(canvas);
        PanBy(_panDragSurface, point.X - last.X, point.Y - last.Y);
        _panDragLast = point;
    }

    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || sender is not SKElement canvas || _panDragLast is null) return;
        _panDragLast = null;
        _panDragSurface = -1;
        canvas.ReleaseMouseCapture();
        canvas.ClearValue(CursorProperty);
        e.Handled = true;
    }

    private void SelectAt(SKElement canvas, int surface, Point pointer)
    {
        if (_descriptor is null) return;

        var renderSize = _renderSizes[surface];
        var renderWidth = renderSize.Width > 0 ? renderSize.Width : Math.Max(1, (int)Math.Round(canvas.ActualWidth));
        var renderHeight = renderSize.Height > 0 ? renderSize.Height : Math.Max(1, (int)Math.Round(canvas.ActualHeight));
        var point = MapPointerToRender(
            new Point(pointer.X - _pan[surface].X, pointer.Y - _pan[surface].Y),
            canvas.ActualWidth,
            canvas.ActualHeight,
            renderWidth,
            renderHeight);
        var sector = _renderer.HitTest(
            _model,
            surface,
            _descriptor.Direction,
            renderWidth,
            renderHeight,
            point,
            _zooms[surface]);
        if (sector is null) return;
        _selectedPositions[surface] = sector.Position;
        UpdateSelectionLabel(surface, sector);
        canvas.InvalidateVisual();
        ElementSelected?.Invoke(surface, sector.Cylinder);
        SectorSelected?.Invoke(surface, sector);
    }

    private void PanBy(int surface, double deltaX, double deltaY)
    {
        var canvas = CanvasFor(surface);
        var maxX = Math.Max(0, canvas.ActualWidth * (_zooms[surface] - 1) / 2);
        var maxY = Math.Max(0, canvas.ActualHeight * (_zooms[surface] - 1) / 2);
        _pan[surface] = new Point(
            Math.Clamp(_pan[surface].X + deltaX, -maxX, maxX),
            Math.Clamp(_pan[surface].Y + deltaY, -maxY, maxY));
        canvas.InvalidateVisual();
    }

    private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        var surface = SurfaceFor(sender);
        SetZoom(surface, _zooms[Math.Clamp(surface, 0, 1)] * (e.Delta > 0 ? 1.12f : .89f));
        e.Handled = true;
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
    {
        var surface = SurfaceFor(sender);
        SetZoom(surface, _zooms[Math.Clamp(surface, 0, 1)] * .89f);
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e)
    {
        var surface = SurfaceFor(sender);
        SetZoom(surface, _zooms[Math.Clamp(surface, 0, 1)] * 1.12f);
    }

    private void ResetZoomButton_Click(object sender, RoutedEventArgs e) => SetZoom(SurfaceFor(sender), 1);

    private void SetZoom(int surface, float zoom)
    {
        if (surface is < 0 or > 1) return;
        var value = Math.Clamp(zoom, .65f, 4f);
        _zooms[surface] = value;
        if (value <= 1) _pan[surface] = new Point();
        UpdateZoomLabel(surface);
        CanvasFor(surface).InvalidateVisual();
        if (!LinkZoom) return;
        var otherSurface = 1 - surface;
        _zooms[otherSurface] = value;
        if (value <= 1) _pan[otherSurface] = new Point();
        UpdateZoomLabel(otherSurface);
        CanvasFor(otherSurface).InvalidateVisual();
    }

    private void UpdateSurfaceLabels()
    {
        Surface0Label.Text = LocExtension.Get("Visual.Side", 0);
        Surface1Label.Text = LocExtension.Get("Visual.Side", 1);
    }

    private void UpdateZoomLabel(int surface) => ResetButtonFor(surface).Content = $"{_zooms[surface]:P0}";

    private void UpdateSelectionLabel(int surface, SectorMediaElement? sector) => SelectionLabelFor(surface).Text = sector is null
        ? $"{LocExtension.Get("Visual.TrackLabel")} — · {LocExtension.Get("Visual.SectorLabel")} —"
        : $"{LocExtension.Get("Visual.TrackLabel")} {sector.Cylinder} · {LocExtension.Get("Visual.SectorLabel")} {sector.Number}";

    private SKElement CanvasFor(int surface) => surface == 0 ? Canvas0 : Canvas1;
    private Button ResetButtonFor(int surface) => surface == 0 ? ResetZoom0Button : ResetZoom1Button;
    private TextBlock SelectionLabelFor(int surface) => surface == 0 ? Selection0Label : Selection1Label;

    private static int SurfaceFor(object? source) => source is FrameworkElement { Tag: string text }
        && int.TryParse(text, out var surface) ? surface : -1;

    internal static SKPoint MapPointerToRender(
        Point point,
        double actualWidth,
        double actualHeight,
        int renderWidth,
        int renderHeight)
    {
        var width = renderWidth > 0 ? renderWidth : Math.Max(1, (int)Math.Round(actualWidth));
        var height = renderHeight > 0 ? renderHeight : Math.Max(1, (int)Math.Round(actualHeight));
        var scaleX = actualWidth > 0 ? width / actualWidth : 1;
        var scaleY = actualHeight > 0 ? height / actualHeight : 1;
        return new SKPoint((float)(point.X * scaleX), (float)(point.Y * scaleY));
    }
}
