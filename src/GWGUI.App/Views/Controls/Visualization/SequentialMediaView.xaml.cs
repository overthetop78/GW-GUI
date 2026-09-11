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
        FaceSelector.Visibility = model.Segments.Any(item => item.FaceNumber.HasValue) ? Visibility.Visible : Visibility.Collapsed;
        TrackSelector.Visibility = model.Segments.Any(item => item.TrackNumber.HasValue) ? Visibility.Visible : Visibility.Collapsed;
        ChannelSelector.Visibility = model.Segments.Any(item => item.ChannelNumber.HasValue) ? Visibility.Visible : Visibility.Collapsed;
        SelectionLabel.Text = string.Empty;
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
        var filtered = FilteredModel();
        _renderer.Render(
            e.Surface.Canvas,
            filtered,
            _selectedSegment,
            Math.Min(_preparedSegmentCount, filtered?.Segments.Count ?? 0),
            SequentialMediaRenderConstants.DefaultWrappedLineCount,
            e.Info.Width,
            e.Info.Height);
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var filtered = FilteredModel();
        var point = e.GetPosition(Canvas);
        var segment = _renderer.HitTest(
            filtered,
            SequentialMediaRenderConstants.DefaultWrappedLineCount,
            Math.Max(1, (int)Canvas.ActualWidth),
            Math.Max(1, (int)Canvas.ActualHeight),
            new SKPoint((float)point.X, (float)point.Y));
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
        SelectionLabel.Text = string.Empty;
        Canvas.InvalidateVisual();
        SegmentSelected?.Invoke(null);
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoom * .8f);
    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoom * 1.25f);
    private void ResetZoomButton_Click(object sender, RoutedEventArgs e) => SetZoom(1);

    private void SetZoom(float zoom)
    {
        var oldScrollableWidth = Math.Max(1, Canvas.ActualWidth - TimelineScrollViewer.ViewportWidth);
        var relativeOffset = TimelineScrollViewer.HorizontalOffset / oldScrollableWidth;
        _zoom = Math.Clamp(zoom, 1, 8);
        ResetZoomButton.Content = $"{_zoom:P0}";
        UpdateCanvasWidth();
        Dispatcher.BeginInvoke(() => TimelineScrollViewer.ScrollToHorizontalOffset(
            relativeOffset * Math.Max(0, Canvas.ActualWidth - TimelineScrollViewer.ViewportWidth)), DispatcherPriority.Loaded);
    }

    private void UpdateCanvasWidth()
    {
        Canvas.Width = Math.Max(640, TimelineScrollViewer.ViewportWidth) * _zoom;
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

    private void UpdateSelectionLabel() => SelectionLabel.Text = _selectedSegment is null
        ? string.Empty
        : $"{_selectedSegment.Start:g} · {_selectedSegment.Duration:g}";
}
