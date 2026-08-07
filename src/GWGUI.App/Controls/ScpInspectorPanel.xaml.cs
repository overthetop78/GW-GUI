using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GWGUI.App.Controls;

public partial class ScpInspectorPanel : UserControl
{
    public ScpInspectorPanel() => InitializeComponent();
    public event EventHandler? CloseRequested;
    public event EventHandler? DetachRequested;
    public event EventHandler? AttachRequested;
    public event EventHandler<DragDeltaEventArgs>? DragRequested;
    public bool IsDetached { set { DetachButton.Visibility = value ? Visibility.Collapsed : Visibility.Visible; AttachButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed; } }
    private void MoveThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) => DragRequested?.Invoke(this, new(e.HorizontalChange, e.VerticalChange));
    private void Close_Click(object sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);
    private void Detach_Click(object sender, RoutedEventArgs e) => DetachRequested?.Invoke(this, EventArgs.Empty);
    private void Attach_Click(object sender, RoutedEventArgs e) => AttachRequested?.Invoke(this, EventArgs.Empty);
}

public sealed record DragDeltaEventArgs(double X, double Y);
