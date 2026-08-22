using GWGUI.App.Contracts.Progress;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace GWGUI.App.ViewModels.Visualization;

public sealed class TrackSegment(int cylinder, int head, Brush brush) : INotifyPropertyChanged
{
    private Brush _brush = brush;

    public int Cylinder { get; } = cylinder;
    public int Head { get; } = head;
    public TrackSegmentState State { get; private set; }
    public Brush Brush
    {
        get => _brush;
        private set
        {
            _brush = value;
            OnPropertyChanged();
        }
    }

    public void SetState(TrackSegmentState state, Brush brush)
    {
        State = state;
        Brush = brush;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new(name));
}
