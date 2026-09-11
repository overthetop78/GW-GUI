using GWGUI.App.Contracts.Rendering.Blocks;
using GWGUI.App.Contracts.Views.Visualization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Rendering.Blocks;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
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

    private void LogicalCanvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e) =>
        _logicalRenderer.Render(e.Surface.Canvas, _model, _selectedRange, e.Info.Width, e.Info.Height, _zoom);

    private void GeometryCanvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e) =>
        _geometryRenderer.Render(e.Surface.Canvas, _model, e.Info.Width, e.Info.Height, _selectedSurface, _zoom);

    private void LogicalCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var point = e.GetPosition(LogicalCanvas);
        var range = _logicalRenderer.HitTest(
            _model,
            Math.Max(1, (int)LogicalCanvas.ActualWidth),
            Math.Max(1, (int)LogicalCanvas.ActualHeight),
            new SKPoint((float)point.X, (float)point.Y),
            _zoom);
        if (range is null) return;
        _selectedRange = range;
        UpdateSelectionLabel();
        LogicalCanvas.InvalidateVisual();
        ElementSelected?.Invoke(0, range.Start);
        RangeSelected?.Invoke(range);
    }

    private void GeometryCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var point = e.GetPosition(GeometryCanvas);
        var surface = _geometryRenderer.HitTest(
            _model,
            Math.Max(1, (int)GeometryCanvas.ActualWidth),
            Math.Max(1, (int)GeometryCanvas.ActualHeight),
            new SKPoint((float)point.X, (float)point.Y));
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
        ResetZoomButton.Content = $"{_zoom:P0}";
        InvalidateCanvases();
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
