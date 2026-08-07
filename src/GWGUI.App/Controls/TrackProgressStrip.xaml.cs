using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.App.Controls;

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

public enum TrackSegmentState { Pending, Active, Success, Retry, Failed }

public sealed class TrackSegment(int cylinder, int head, Brush brush) : INotifyPropertyChanged
{
    private Brush _brush = brush;
    public int Cylinder { get; } = cylinder;
    public int Head { get; } = head;
    public TrackSegmentState State { get; private set; }
    public Brush Brush { get => _brush; private set { _brush = value; OnPropertyChanged(); } }
    public void SetState(TrackSegmentState state, Brush brush)
    {
        State = state;
        Brush = brush;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}
