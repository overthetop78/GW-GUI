using System.Windows.Controls;

namespace GWGUI.App.Controls;

public partial class OptionsEnginesSection : UserControl
{
    public OptionsEnginesSection() => InitializeComponent();

    public ComboBox PhysicalRead => PhysicalReadCombo;
    public ComboBox PhysicalWrite => PhysicalWriteCombo;
    public ComboBox Conversion => ConversionCombo;
    public ComboBox ExplorerRead => ExplorerReadCombo;

    public event SelectionChangedEventHandler? EngineChanged;

    private void Engine_SelectionChanged(object sender, SelectionChangedEventArgs e) => EngineChanged?.Invoke(sender, e);
}
