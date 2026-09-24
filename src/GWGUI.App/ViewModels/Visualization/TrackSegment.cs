using GWGUI.App.Contracts.Progress;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace GWGUI.App.ViewModels.Visualization;

public sealed class TrackSegment(long position, int surface, Brush brush) : INotifyPropertyChanged
{
    private Brush _brush = brush;
    private bool _isSelected;

    public long Position { get; } = position;
    public int Surface { get; } = surface;
    public TrackSegmentState State { get; private set; }
    public bool IsSelected
    {
        get => _isSelected;
        private set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

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

    public void SetSelected(bool selected) => IsSelected = selected;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new(name));
}
