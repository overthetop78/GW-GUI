using GWGUI.App.Contracts.Rendering.Blocks;
using GWGUI.App.Contracts.Views.Visualization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Rendering.Blocks;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class BlockMediaView : UserControl, IMediaVisualizationView
{
    private readonly SkiaBlockMediaRenderer _logicalRenderer = new();
    private readonly SkiaHardDiskGeometryRenderer _geometryRenderer = new();
    private BlockMediaRenderModel? _model;
    private BlockMediaRange? _selectedRange;
    private int? _selectedSurface;
    private float _zoom = 1;
    private Point _pan;
    private Point? _panDragLast;
    private SKElement? _dragCanvas;
    private (int Width, int Height) _logicalRenderSize;
    private (int Width, int Height) _geometryRenderSize;

    public BlockMediaView()
    {
        InitializeComponent();
        SurfaceSelector.DisplayMemberPath = "Value";
    }

    public event Action<int, long>? ElementSelected;
    public event Action<BlockMediaRange?>? RangeSelected;
    public event Action<int?>? SurfaceSelected;

    public void SetDocument(BlockMediaRenderModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
        _selectedRange = null;
        _selectedSurface = null;
        _pan = new Point();
        GeometryTab.Visibility = model.Geometry is null ? Visibility.Collapsed : Visibility.Visible;
        var surfaces = model.Geometry is null
            ? []
            : Enumerable.Range(0, model.Geometry.Heads)
                .Select(index => new KeyValuePair<int, string>(index, LocExtension.Get("Visual.Surface", index)))
                .ToArray();
        SurfaceSelector.ItemsSource = surfaces;
        SurfaceSelector.SelectedIndex = surfaces.Length > 0 ? 0 : -1;
        UpdateSelectionLabel();
        InvalidateCanvases();
    }

    public void SelectElement(int surface, long position)
    {
        _selectedRange = _model?.Ranges.FirstOrDefault(range => range.Start == position);
        UpdateSelectionLabel();
        LogicalCanvas.InvalidateVisual();
    }

    private void LogicalCanvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        _logicalRenderSize = (e.Info.Width, e.Info.Height);
        RenderTranslated(e.Surface.Canvas, LogicalCanvas, e.Info.Width, e.Info.Height,
            () => _logicalRenderer.Render(e.Surface.Canvas, _model, _selectedRange, e.Info.Width, e.Info.Height, _zoom));
    }

    private void GeometryCanvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        _geometryRenderSize = (e.Info.Width, e.Info.Height);
        RenderTranslated(e.Surface.Canvas, GeometryCanvas, e.Info.Width, e.Info.Height,
            () => _geometryRenderer.Render(e.Surface.Canvas, _model, e.Info.Width, e.Info.Height, _selectedSurface, _zoom));
    }

    private void LogicalCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        SelectLogicalAt(e.GetPosition(LogicalCanvas));

    private void GeometryCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        SelectGeometryAt(e.GetPosition(GeometryCanvas));

    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || sender is not SKElement canvas || _zoom <= 1) return;
        _dragCanvas = canvas;
        _panDragLast = e.GetPosition(canvas);
        canvas.Cursor = VisualizationCursors.Grabbing;
        canvas.CaptureMouse();
        e.Handled = true;
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not SKElement canvas || !ReferenceEquals(canvas, _dragCanvas)
            || _panDragLast is not Point last || e.MiddleButton != MouseButtonState.Pressed) return;
        var point = e.GetPosition(canvas);
        PanBy(canvas, point.X - last.X, point.Y - last.Y);
        _panDragLast = point;
    }

    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || sender is not SKElement canvas
            || !ReferenceEquals(canvas, _dragCanvas) || _panDragLast is null) return;
        _dragCanvas = null;
        _panDragLast = null;
        canvas.ReleaseMouseCapture();
        canvas.ClearValue(CursorProperty);
        e.Handled = true;
    }

    private void SelectLogicalAt(Point pointer)
    {
        var point = MapPointer(LogicalCanvas, _logicalRenderSize, pointer);
        var range = _logicalRenderer.HitTest(
            _model,
            Math.Max(1, _logicalRenderSize.Width),
            Math.Max(1, _logicalRenderSize.Height),
            point,
            _zoom);
        if (range is null) return;
        _selectedRange = range;
        UpdateSelectionLabel();
        LogicalCanvas.InvalidateVisual();
        ElementSelected?.Invoke(0, range.Start);
        RangeSelected?.Invoke(range);
    }

    private void SelectGeometryAt(Point pointer)
    {
        var point = MapPointer(GeometryCanvas, _geometryRenderSize, pointer);
        var surface = _geometryRenderer.HitTest(
            _model,
            Math.Max(1, _geometryRenderSize.Width),
            Math.Max(1, _geometryRenderSize.Height),
            point);
        if (surface is null) return;
        _selectedSurface = surface;
        SurfaceSelector.SelectedItem = SurfaceSelector.Items.Cast<KeyValuePair<int, string>>()
            .FirstOrDefault(item => item.Key == surface);
        GeometryCanvas.InvalidateVisual();
        SurfaceSelected?.Invoke(surface);
    }

    private void SurfaceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedSurface = SurfaceSelector.SelectedItem is KeyValuePair<int, string> choice ? choice.Key : null;
        GeometryCanvas.InvalidateVisual();
        SurfaceSelected?.Invoke(_selectedSurface);
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoom * .89f);
    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoom * 1.12f);
    private void ResetZoomButton_Click(object sender, RoutedEventArgs e) => SetZoom(1);

    private void SetZoom(float zoom)
    {
        _zoom = Math.Clamp(zoom, .65f, 4f);
        if (_zoom <= 1) _pan = new Point();
        ResetZoomButton.Content = $"{_zoom:P0}";
        InvalidateCanvases();
    }

    private void PanBy(SKElement canvas, double deltaX, double deltaY)
    {
        var maxX = Math.Max(0, canvas.ActualWidth * (_zoom - 1) / 2);
        var maxY = Math.Max(0, canvas.ActualHeight * (_zoom - 1) / 2);
        _pan = new Point(
            Math.Clamp(_pan.X + deltaX, -maxX, maxX),
            Math.Clamp(_pan.Y + deltaY, -maxY, maxY));
        InvalidateCanvases();
    }

    private void RenderTranslated(SKCanvas canvas, SKElement element, int width, int height, Action render)
    {
        canvas.Save();
        canvas.Translate(
            (float)(_pan.X * width / Math.Max(1, element.ActualWidth)),
            (float)(_pan.Y * height / Math.Max(1, element.ActualHeight)));
        render();
        canvas.Restore();
    }

    private SKPoint MapPointer(SKElement canvas, (int Width, int Height) renderSize, Point pointer)
    {
        var width = Math.Max(1, renderSize.Width);
        var height = Math.Max(1, renderSize.Height);
        return new SKPoint(
            (float)((pointer.X - _pan.X) * width / Math.Max(1, canvas.ActualWidth)),
            (float)((pointer.Y - _pan.Y) * height / Math.Max(1, canvas.ActualHeight)));
    }

    private void UpdateSelectionLabel() => SelectionLabel.Text = _selectedRange is null
        ? string.Empty
        : $"LBA {_selectedRange.Start:N0} · {_selectedRange.Length:N0}";

    private void InvalidateCanvases()
    {
        LogicalCanvas.InvalidateVisual();
        GeometryCanvas.InvalidateVisual();
    }
}
