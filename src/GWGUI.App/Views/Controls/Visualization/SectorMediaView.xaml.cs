using GWGUI.App.Contracts.Rendering.Sectors;
using GWGUI.App.Contracts.Views.Visualization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Rendering.Sectors;
using GWGUI.MediaEngine.Visualization;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class SectorMediaView : UserControl, IMediaVisualizationView
{
    private readonly SkiaSectorMediaRenderer _renderer = new();
    private SectorMediaRenderModel? _model;
    private MediaVisualizationDescriptor? _descriptor;
    private int _surface;
    private long? _selectedPosition;
    private float _zoom = 1;

    public SectorMediaView()
    {
        InitializeComponent();
        SurfaceSelector.DisplayMemberPath = "Value";
    }

    public event Action<int, long>? ElementSelected;
    public event Action<int, SectorMediaElement?>? SectorSelected;

    public void SetDocument(SectorMediaRenderModel model, MediaVisualizationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(descriptor);
        _model = model;
        _descriptor = descriptor;
        var choices = model.Surfaces
            .Select(surface => new KeyValuePair<int, string>(surface.Index, LocExtension.Get("Visual.Side", surface.Index)))
            .ToArray();
        SurfaceSelector.ItemsSource = choices;
        SurfaceSelector.SelectedIndex = choices.Length > 0 ? 0 : -1;
        _surface = choices.FirstOrDefault().Key;
        _selectedPosition = null;
        UpdateSelectionLabel(null);
        Canvas.InvalidateVisual();
    }

    public void SelectElement(int surface, long position)
    {
        if (_model?.Surfaces.All(item => item.Index != surface) != false) return;
        _surface = surface;
        SurfaceSelector.SelectedItem = SurfaceSelector.Items.Cast<KeyValuePair<int, string>>()
            .FirstOrDefault(item => item.Key == surface);
        var sector = _model.Surfaces.First(item => item.Index == surface).Tracks
            .FirstOrDefault(track => track.Cylinder == position)?.Sectors.FirstOrDefault();
        _selectedPosition = sector?.Position;
        UpdateSelectionLabel(sector);
        Canvas.InvalidateVisual();
    }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (_descriptor is null)
        {
            e.Surface.Canvas.Clear(SKColors.Black);
            return;
        }

        _renderer.Render(
            e.Surface.Canvas,
            _model,
            _surface,
            _descriptor.Direction,
            _selectedPosition,
            e.Info.Width,
            e.Info.Height,
            _zoom);
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_descriptor is null) return;
        var point = e.GetPosition(Canvas);
        var scaleX = Canvas.ActualWidth <= 0 ? 1 : Canvas.DesiredSize.Width / Canvas.ActualWidth;
        var scaleY = Canvas.ActualHeight <= 0 ? 1 : Canvas.DesiredSize.Height / Canvas.ActualHeight;
        var sector = _renderer.HitTest(
            _model,
            _surface,
            _descriptor.Direction,
            Math.Max(1, (int)Canvas.ActualWidth),
            Math.Max(1, (int)Canvas.ActualHeight),
            new SKPoint((float)(point.X * scaleX), (float)(point.Y * scaleY)),
            _zoom);
        if (sector is null) return;
        _selectedPosition = sector.Position;
        UpdateSelectionLabel(sector);
        Canvas.InvalidateVisual();
        ElementSelected?.Invoke(_surface, sector.Cylinder);
        SectorSelected?.Invoke(_surface, sector);
    }

    private void SurfaceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SurfaceSelector.SelectedItem is not KeyValuePair<int, string> choice) return;
        _surface = choice.Key;
        _selectedPosition = null;
        UpdateSelectionLabel(null);
        Canvas.InvalidateVisual();
        SectorSelected?.Invoke(_surface, null);
    }

    private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        SetZoom(_zoom * (e.Delta > 0 ? 1.12f : .89f));
        e.Handled = true;
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoom * .89f);
    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoom * 1.12f);
    private void ResetZoomButton_Click(object sender, RoutedEventArgs e) => SetZoom(1);

    private void SetZoom(float zoom)
    {
        _zoom = Math.Clamp(zoom, .65f, 4f);
        ResetZoomButton.Content = $"{_zoom:P0}";
        Canvas.InvalidateVisual();
    }

    private SectorMediaElement? FindSector(int surface, long position) => _model?.Surfaces
        .FirstOrDefault(item => item.Index == surface)?.Tracks
        .SelectMany(track => track.Sectors)
        .FirstOrDefault(sector => sector.Position == position);

    private void UpdateSelectionLabel(SectorMediaElement? sector) => SelectionLabel.Text = sector is null
        ? string.Empty
        : $"{LocExtension.Get("Visual.TrackLabel")} {sector.Position} · {LocExtension.Get("Visual.SectorLabel")} {sector.Number}";
}
