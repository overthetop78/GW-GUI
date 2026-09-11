using GWGUI.App.Contracts.Rendering.Optical;
using GWGUI.App.Contracts.Views.Visualization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Rendering.Optical;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class OpticalMediaView : UserControl, IMediaVisualizationView
{
    private readonly SkiaOpticalMediaRenderer _renderer = new();
    private OpticalMediaRenderModel? _model;
    private OpticalMediaTrack? _selectedTrack;
    private float _zoom = 1;

    public OpticalMediaView()
    {
        InitializeComponent();
        FaceSelector.DisplayMemberPath = "Value";
        LayerSelector.DisplayMemberPath = "Value";
        SessionSelector.DisplayMemberPath = "Value";
    }

    public event Action<int, long>? ElementSelected;
    public event Action<OpticalMediaTrack?>? TrackSelected;

    public void SetDocument(OpticalMediaRenderModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
        _selectedTrack = null;
        SetChoices(FaceSelector, model.FaceCount is > 1 ? Enumerable.Range(0, model.FaceCount.Value) : [] , "Visual.DiscFace");
        SetChoices(LayerSelector, model.LayerCount is > 1 ? Enumerable.Range(0, model.LayerCount.Value) : [], "Visual.Layer");
        SetChoices(SessionSelector, model.Tracks.Select(track => track.SessionNumber).Distinct().Order(), "Visual.Session");
        FaceSelector.Visibility = model.FaceCount is > 1 ? Visibility.Visible : Visibility.Collapsed;
        LayerSelector.Visibility = model.LayerCount is > 1 ? Visibility.Visible : Visibility.Collapsed;
        SelectionLabel.Text = string.Empty;
        Canvas.InvalidateVisual();
    }

    public void SelectElement(int surface, long position)
    {
        _selectedTrack = _model?.Tracks.FirstOrDefault(track =>
            track.FirstSector == position && (track.FaceNumber is null || track.FaceNumber == surface));
        UpdateSelectionLabel();
        Canvas.InvalidateVisual();
    }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e) => _renderer.Render(
        e.Surface.Canvas,
        _model,
        _selectedTrack,
        SelectedValue(FaceSelector),
        SelectedValue(LayerSelector),
        SelectedValue(SessionSelector),
        e.Info.Width,
        e.Info.Height,
        _zoom);

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var point = e.GetPosition(Canvas);
        var track = _renderer.HitTest(
            _model,
            SelectedValue(FaceSelector),
            SelectedValue(LayerSelector),
            SelectedValue(SessionSelector),
            Math.Max(1, (int)Canvas.ActualWidth),
            Math.Max(1, (int)Canvas.ActualHeight),
            new SKPoint((float)point.X, (float)point.Y),
            _zoom);
        if (track is null) return;
        _selectedTrack = track;
        UpdateSelectionLabel();
        Canvas.InvalidateVisual();
        ElementSelected?.Invoke(track.FaceNumber ?? 0, track.FirstSector);
        TrackSelected?.Invoke(track);
    }

    private void FilterSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedTrack = null;
        SelectionLabel.Text = string.Empty;
        Canvas.InvalidateVisual();
        TrackSelected?.Invoke(null);
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

    private static int? SelectedValue(ComboBox selector) =>
        selector.SelectedItem is KeyValuePair<int?, string> choice ? choice.Key : null;

    private static void SetChoices(ComboBox selector, IEnumerable<int> values, string resourceKey)
    {
        var choices = new List<KeyValuePair<int?, string>>
        {
            new(null, LocExtension.Get("Visual.All"))
        };
        choices.AddRange(values.Select(value => new KeyValuePair<int?, string>(value, LocExtension.Get(resourceKey, value))));
        selector.ItemsSource = choices;
        selector.SelectedIndex = 0;
    }

    private void UpdateSelectionLabel() => SelectionLabel.Text = _selectedTrack is null
        ? string.Empty
        : $"{LocExtension.Get("Visual.SessionLabel")} {_selectedTrack.SessionNumber} · {LocExtension.Get("Visual.TrackLabel")} {_selectedTrack.TrackNumber}";
}
