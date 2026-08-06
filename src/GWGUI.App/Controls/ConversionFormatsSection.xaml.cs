using System.Windows.Controls;

namespace GWGUI.App.Controls;

public partial class ConversionFormatsSection : UserControl
{
    public ConversionFormatsSection() => InitializeComponent();
    public event EventHandler? ValueChanged;
    public ItemsControl PinnedItems => Pinned;
    public ItemsControl CommonItems => Common;
    public ItemsControl RareItems => Rare;
    private void Format_ValueChanged(object? sender, EventArgs e) => ValueChanged?.Invoke(sender, e);
}
