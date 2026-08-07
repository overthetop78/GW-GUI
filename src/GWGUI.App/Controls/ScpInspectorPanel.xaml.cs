using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Controls;

public partial class ScpInspectorPanel : UserControl
{
    public ScpInspectorPanel() => InitializeComponent();
    public event EventHandler? CloseRequested;
    public event EventHandler? DetachRequested;
    public event EventHandler? AttachRequested;
    public event EventHandler<DragDeltaEventArgs>? DragRequested;
    public bool IsDetached { set { DetachButton.Visibility = value ? Visibility.Collapsed : Visibility.Visible; AttachButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed; } }
    private Point? _dragOrigin;
    private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { _dragOrigin = e.GetPosition(this); DragArea.CaptureMouse(); DragArea.MouseMove += DragArea_MouseMove; DragArea.MouseLeftButtonUp += DragArea_MouseLeftButtonUp; }
    private void DragArea_MouseMove(object sender, MouseEventArgs e) { if (_dragOrigin is not { } origin || e.LeftButton != MouseButtonState.Pressed) return; var point=e.GetPosition(this); DragRequested?.Invoke(this,new(point.X-origin.X,point.Y-origin.Y)); _dragOrigin=point; }
    private void DragArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { _dragOrigin=null; DragArea.ReleaseMouseCapture(); DragArea.MouseMove-=DragArea_MouseMove; DragArea.MouseLeftButtonUp-=DragArea_MouseLeftButtonUp; }
    private void Close_Click(object sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);
    private void Detach_Click(object sender, RoutedEventArgs e) => DetachRequested?.Invoke(this, EventArgs.Empty);
    private void Attach_Click(object sender, RoutedEventArgs e) => AttachRequested?.Invoke(this, EventArgs.Empty);
}

public sealed record DragDeltaEventArgs(double X, double Y);
