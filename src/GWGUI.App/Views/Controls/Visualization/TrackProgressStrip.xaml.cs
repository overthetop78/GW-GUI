using GWGUI.App.Contracts.Progress;
using GWGUI.App.ViewModels.Visualization;
using GWGUI.MediaEngine.Enums;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace GWGUI.App.Views.Controls.Visualization;

public partial class TrackProgressStrip : UserControl
{
    private static readonly Brush PendingBrush = Freeze(Color.FromRgb(190, 194, 199));
    private static readonly Brush ActiveBrush = Freeze(Color.FromRgb(62, 132, 210));
    private static readonly Brush SuccessBrush = Freeze(Color.FromRgb(60, 166, 91));
    private static readonly Brush RetryBrush = Freeze(Color.FromRgb(224, 151, 47));
    private static readonly Brush FailedBrush = Freeze(Color.FromRgb(207, 67, 67));

    public ObservableCollection<TrackSegment> Segments { get; } = [];
    public int Head { get; set; }
    public MediaVisualizationProgressUnit Unit { get; private set; } = MediaVisualizationProgressUnit.Track;
    public int Total => Segments.Count;
    public int Completed => Segments.Count(item => item.State is TrackSegmentState.Success or TrackSegmentState.Failed);
    public event Action<int, long>? ElementSelected;

    public TrackProgressStrip() => InitializeComponent();

    public void Configure(int head, IReadOnlyList<int> cylinders, string label)
        => Configure(MediaVisualizationProgressUnit.Track, head, cylinders.Select(value => (long)value).ToArray(), label);

    public void Configure(
        MediaVisualizationProgressUnit unit,
        int surface,
        IReadOnlyList<long> elements,
        string label)
    {
        ArgumentNullException.ThrowIfNull(elements);
        ArgumentNullException.ThrowIfNull(label);
        Unit = unit;
        Head = surface;
        ProgressLabel.Text = label;
        ProgressLabel.ToolTip = label;
        Segments.Clear();
        foreach (var element in elements)
            Segments.Add(new TrackSegment(element, surface, PendingBrush));
        UpdateCount();
    }

    public void SetState(long position, TrackSegmentState state)
    {
        var segment = Segments.FirstOrDefault(item => item.Position == position);
        if (segment is null) return;
        segment.SetState(state, state switch
        {
            TrackSegmentState.Active => ActiveBrush,
            TrackSegmentState.Success => SuccessBrush,
            TrackSegmentState.Retry => RetryBrush,
            TrackSegmentState.Failed => FailedBrush,
            _ => PendingBrush
        });
        UpdateCount();
    }

    public void SetColor(long position, Color color)
    {
        var segment = Segments.FirstOrDefault(item => item.Position == position);
        if (segment is null) return;
        segment.SetState(TrackSegmentState.Success, Freeze(color));
        UpdateCount();
    }

    public void SetActive(long position)
    {
        ClearActive();
        SetState(position, TrackSegmentState.Active);
    }

    public void Select(long position)
    {
        foreach (var item in Segments)
            item.SetSelected(item.Position == position);
    }

    public void ClearActive()
    {
        foreach (var item in Segments.Where(item => item.State == TrackSegmentState.Active))
            item.SetState(TrackSegmentState.Pending, PendingBrush);
    }

    public void ResetToPending()
    {
        foreach (var item in Segments)
            item.SetState(TrackSegmentState.Pending, PendingBrush);
        UpdateCount();
    }

    public void Reset()
    {
        Segments.Clear();
        UpdateCount();
    }

    private void Segment_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { CommandParameter: TrackSegment segment }) return;
        Select(segment.Position);
        ElementSelected?.Invoke(segment.Surface, segment.Position);
    }

    private void UpdateCount() => ProgressCount.Text = $"{Completed} / {Total}";

    private static Brush Freeze(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
