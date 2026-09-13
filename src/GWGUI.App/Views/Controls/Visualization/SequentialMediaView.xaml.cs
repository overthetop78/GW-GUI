using GWGUI.App.Constants.Rendering.Sequential;
using GWGUI.App.Contracts.Rendering.Sequential;
using GWGUI.App.Contracts.Views.Visualization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Rendering.Sequential;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class SequentialMediaView : UserControl, IMediaVisualizationView
{
    private readonly SkiaSequentialMediaRenderer _renderer = new();
    private CancellationTokenSource? _preparationCancellation;
    private SequentialMediaRenderModel? _model;
    private SequentialMediaSegment? _selectedSegment;
    private int _preparedSegmentCount;
    private float _zoom = 1;
    private int _renderPixelWidth = 1;
    private int _renderPixelHeight = 1;
    private Point? _panDragLast;

    public SequentialMediaView()
    {
        InitializeComponent();
        FaceSelector.DisplayMemberPath = "Value";
        TrackSelector.DisplayMemberPath = "Value";
        ChannelSelector.DisplayMemberPath = "Value";
        SizeChanged += (_, _) => UpdateCanvasWidth();
    }

    public event Action<int, long>? ElementSelected;
    public event Action<SequentialMediaSegment?>? SegmentSelected;

    public void SetDocument(SequentialMediaRenderModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _preparationCancellation?.Cancel();
        _preparationCancellation?.Dispose();
        _preparationCancellation = new CancellationTokenSource();
        _model = model;
        _selectedSegment = null;
        _preparedSegmentCount = 0;
        SetChoices(FaceSelector, model.Segments.Where(item => item.FaceNumber.HasValue).Select(item => item.FaceNumber!.Value), "Visual.DiscFace");
        SetChoices(TrackSelector, model.Segments.Where(item => item.TrackNumber.HasValue).Select(item => item.TrackNumber!.Value), "Visual.TrackNumber");
        SetChoices(ChannelSelector, model.Segments.Where(item => item.ChannelNumber.HasValue).Select(item => item.ChannelNumber!.Value), "Visual.Channel");
        SetFilterVisibility(FaceLabel, FaceSelector, model.Segments.Any(item => item.FaceNumber.HasValue));
        SetFilterVisibility(TrackLabel, TrackSelector, model.Segments.Any(item => item.TrackNumber.HasValue));
        SetFilterVisibility(ChannelLabel, ChannelSelector, model.Segments.Any(item => item.ChannelNumber.HasValue));
        UpdateSelectionLabel();
        UpdateCanvasWidth();
        _ = PrepareProgressivelyAsync(_preparationCancellation.Token);
    }

    public void SelectElement(int surface, long position)
    {
        _selectedSegment = _model?.Segments.FirstOrDefault(segment => segment.Lane == surface && segment.Position == position);
        UpdateSelectionLabel();
        Canvas.InvalidateVisual();
    }

    private async Task PrepareProgressivelyAsync(CancellationToken cancellationToken)
    {
        var count = _model?.Segments.Count ?? 0;
        try
        {
            while (_preparedSegmentCount < count)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _preparedSegmentCount = Math.Min(count, _preparedSegmentCount + 32);
                Canvas.InvalidateVisual();
                await Dispatcher.Yield(DispatcherPriority.Background);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        _renderPixelWidth = Math.Max(1, e.Info.Width);
        _renderPixelHeight = Math.Max(1, e.Info.Height);
        var filtered = FilteredModel();
        _renderer.Render(
            e.Surface.Canvas,
            filtered,
            _selectedSegment,
            Math.Min(_preparedSegmentCount, filtered?.Segments.Count ?? 0),
            SequentialMediaRenderConstants.DefaultWrappedLineCount,
            e.Info.Width,
            e.Info.Height,
            _zoom);
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => SelectAt(e.GetPosition(Canvas));

    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || _zoom <= 1) return;
        _panDragLast = e.GetPosition(TimelineScrollViewer);
        Canvas.Cursor = VisualizationCursors.Grabbing;
        Canvas.CaptureMouse();
        e.Handled = true;
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_panDragLast is not Point last || e.MiddleButton != MouseButtonState.Pressed) return;
        var point = e.GetPosition(TimelineScrollViewer);
        TimelineScrollViewer.ScrollToHorizontalOffset(TimelineScrollViewer.HorizontalOffset - (point.X - last.X));
        TimelineScrollViewer.ScrollToVerticalOffset(TimelineScrollViewer.VerticalOffset - (point.Y - last.Y));
        _panDragLast = point;
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
        var filtered = FilteredModel();
        var scaleX = _renderPixelWidth / Math.Max(1, Canvas.ActualWidth);
        var scaleY = _renderPixelHeight / Math.Max(1, Canvas.ActualHeight);
        var segment = _renderer.HitTest(
            filtered,
            SequentialMediaRenderConstants.DefaultWrappedLineCount,
            _renderPixelWidth,
            _renderPixelHeight,
            new SKPoint((float)(pointer.X * scaleX), (float)(pointer.Y * scaleY)),
            _zoom);
        if (segment is null) return;
        _selectedSegment = segment;
        UpdateSelectionLabel();
        Canvas.InvalidateVisual();
        ElementSelected?.Invoke(segment.Lane, segment.Position);
        SegmentSelected?.Invoke(segment);
    }

    private void FilterSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedSegment = null;
        UpdateSelectionLabel();
        Canvas.InvalidateVisual();
        SegmentSelected?.Invoke(null);
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoom * .8f);
    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoom * 1.25f);
    private void ResetZoomButton_Click(object sender, RoutedEventArgs e) => SetZoom(1);

    private void TimelineScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        SetZoom(_zoom * (e.Delta > 0 ? 1.15f : 1 / 1.15f), e.GetPosition(TimelineScrollViewer));
        e.Handled = true;
    }

    private void TimelineScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateCanvasWidth();

    private void SetZoom(float zoom, Point? anchor = null)
    {
        var point = anchor ?? new Point(TimelineScrollViewer.ViewportWidth / 2, TimelineScrollViewer.ViewportHeight / 2);
        var oldWidth = Math.Max(1, Canvas.ActualWidth);
        var oldHeight = Math.Max(1, Canvas.ActualHeight);
        var relativeX = (TimelineScrollViewer.HorizontalOffset + point.X) / oldWidth;
        var relativeY = (TimelineScrollViewer.VerticalOffset + point.Y) / oldHeight;
        _zoom = Math.Clamp(zoom, 1, 8);
        ResetZoomButton.Content = $"{_zoom:P0}";
        UpdateCanvasWidth();
        Dispatcher.BeginInvoke(() =>
        {
            TimelineScrollViewer.ScrollToHorizontalOffset(relativeX * Canvas.ActualWidth - point.X);
            TimelineScrollViewer.ScrollToVerticalOffset(relativeY * Canvas.ActualHeight - point.Y);
        }, DispatcherPriority.Loaded);
    }

    private void UpdateCanvasWidth()
    {
        var viewportWidth = Math.Max(TimelineScrollViewer.ViewportWidth, TimelineScrollViewer.ActualWidth - 2);
        Canvas.Width = Math.Max(640, viewportWidth) * _zoom;
        var viewportHeight = Math.Max(TimelineScrollViewer.ViewportHeight, TimelineScrollViewer.ActualHeight - 2);
        Canvas.Height = Math.Max(360, viewportHeight) * _zoom;
        Canvas.InvalidateVisual();
    }

    private SequentialMediaRenderModel? FilteredModel()
    {
        if (_model is null) return null;
        var face = SelectedValue(FaceSelector);
        var track = SelectedValue(TrackSelector);
        var channel = SelectedValue(ChannelSelector);
        var segments = _model.Segments.Where(segment =>
            (face is null || segment.FaceNumber == face) &&
            (track is null || segment.TrackNumber == track) &&
            (channel is null || segment.ChannelNumber == channel)).ToArray();
        return _model with { Segments = segments };
    }

    private static int? SelectedValue(ComboBox selector) =>
        selector.SelectedItem is KeyValuePair<int?, string> choice ? choice.Key : null;

    private static void SetChoices(ComboBox selector, IEnumerable<int> values, string resourceKey)
    {
        var choices = new List<KeyValuePair<int?, string>> { new(null, LocExtension.Get("Visual.All")) };
        choices.AddRange(values.Distinct().Order().Select(value =>
            new KeyValuePair<int?, string>(value, LocExtension.Get(resourceKey, value))));
        selector.ItemsSource = choices;
        selector.SelectedIndex = 0;
    }

    private static void SetFilterVisibility(TextBlock label, ComboBox selector, bool visible)
    {
        var visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        label.Visibility = visibility;
        selector.Visibility = visibility;
    }

    private void UpdateSelectionLabel()
    {
        if (_selectedSegment is not null)
        {
            SelectionLabel.Text = $"{LocExtension.Get("Visual.StartLabel")} {_selectedSegment.Start:g}  ·  {LocExtension.Get("Visual.DurationLabel")} {_selectedSegment.Duration:g}";
            return;
        }

        if (_model is null)
        {
            SelectionLabel.Text = string.Empty;
            return;
        }

        var duration = _model.Duration is { } value
            ? $"  ·  {LocExtension.Get("Visual.DurationLabel")} {value:g}"
            : string.Empty;
        SelectionLabel.Text = $"{LocExtension.Get("Explorer.Cassette")}{duration}  ·  {LocExtension.Get("Visual.SegmentCountLabel")} {_model.Segments.Count}";
    }
}
