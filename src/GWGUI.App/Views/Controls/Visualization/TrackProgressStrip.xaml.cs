using GWGUI.App.Contracts.Progress;
using GWGUI.App.ViewModels.Visualization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

    public TrackProgressStrip() => InitializeComponent();

    public void Configure(int head, IReadOnlyList<int> cylinders, string label)
    {
        Head = head;
        FaceLabel.Text = label;
        Segments.Clear();
        foreach (var cylinder in cylinders)
            Segments.Add(new TrackSegment(cylinder, head, PendingBrush));
    }

    public void SetState(int cylinder, TrackSegmentState state)
    {
        var segment = Segments.FirstOrDefault(item => item.Cylinder == cylinder);
        if (segment is null) return;
        segment.SetState(state, state switch
        {
            TrackSegmentState.Active => ActiveBrush,
            TrackSegmentState.Success => SuccessBrush,
            TrackSegmentState.Retry => RetryBrush,
            TrackSegmentState.Failed => FailedBrush,
            _ => PendingBrush
        });
    }

    public void SetColor(int cylinder, Color color)
    {
        var segment = Segments.FirstOrDefault(item => item.Cylinder == cylinder);
        if (segment is null) return;
        segment.SetState(TrackSegmentState.Success, Freeze(color));
    }

    public void SetActive(int cylinder)
    {
        ClearActive();
        SetState(cylinder, TrackSegmentState.Active);
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
    }

    public void Reset() => Segments.Clear();

    private static Brush Freeze(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
