using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Controls;

public partial class ConversionFormatsSection : UserControl
{
    public ConversionFormatsSection() => InitializeComponent();
    public event EventHandler? ValueChanged;
    public ItemsControl PinnedItems => Pinned;
    public ItemsControl CommonItems => Common;
    public ItemsControl RareItems => Rare;
    private void Format_ValueChanged(object? sender, EventArgs e) => ValueChanged?.Invoke(sender, e);

    private void List_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer viewer || viewer.ScrollableHeight <= 0) return;
        viewer.ScrollToVerticalOffset(Math.Clamp(viewer.VerticalOffset - e.Delta / 3d, 0, viewer.ScrollableHeight));
        e.Handled = true;
    }
}
