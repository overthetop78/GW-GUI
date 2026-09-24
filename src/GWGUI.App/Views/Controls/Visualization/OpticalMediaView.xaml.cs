using GWGUI.App.Contracts.Rendering.Optical;
using GWGUI.App.Contracts.Views.Visualization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Rendering.Optical;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
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
    private Point _pan;
    private Point? _panDragLast;
    private (int Width, int Height) _renderSize;

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
        _pan = new Point();
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

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        _renderSize = (e.Info.Width, e.Info.Height);
        e.Surface.Canvas.Save();
        e.Surface.Canvas.Translate(
            (float)(_pan.X * e.Info.Width / Math.Max(1, Canvas.ActualWidth)),
            (float)(_pan.Y * e.Info.Height / Math.Max(1, Canvas.ActualHeight)));
        _renderer.Render(
            e.Surface.Canvas,
            _model,
            _selectedTrack,
            SelectedValue(FaceSelector),
            SelectedValue(LayerSelector),
            SelectedValue(SessionSelector),
            e.Info.Width,
            e.Info.Height,
            _zoom);
        e.Surface.Canvas.Restore();
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => SelectAt(e.GetPosition(Canvas));

    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || _zoom <= 1) return;
        _panDragLast = e.GetPosition(Canvas);
        Canvas.Cursor = VisualizationCursors.Grabbing;
        Canvas.CaptureMouse();
        e.Handled = true;
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_panDragLast is not Point last || e.MiddleButton != MouseButtonState.Pressed) return;
        var point = e.GetPosition(Canvas);
        var maxX = Math.Max(0, Canvas.ActualWidth * (_zoom - 1) / 2);
        var maxY = Math.Max(0, Canvas.ActualHeight * (_zoom - 1) / 2);
        _pan = new Point(
            Math.Clamp(_pan.X + point.X - last.X, -maxX, maxX),
            Math.Clamp(_pan.Y + point.Y - last.Y, -maxY, maxY));
        _panDragLast = point;
        Canvas.InvalidateVisual();
    }

    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || _panDragLast is null) return;
        _panDragLast = null;
        Canvas.ReleaseMouseCapture();
        Canvas.ClearValue(CursorProperty);
        e.Handled = true;
    }

    private void SelectAt(Point pointer)
    {
        var width = Math.Max(1, _renderSize.Width);
        var height = Math.Max(1, _renderSize.Height);
        var point = new SKPoint(
            (float)((pointer.X - _pan.X) * width / Math.Max(1, Canvas.ActualWidth)),
            (float)((pointer.Y - _pan.Y) * height / Math.Max(1, Canvas.ActualHeight)));
        var track = _renderer.HitTest(
            _model,
            SelectedValue(FaceSelector),
            SelectedValue(LayerSelector),
            SelectedValue(SessionSelector),
            width,
            height,
            point,
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
        if (_zoom <= 1) _pan = new Point();
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
