using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Conversion;

public partial class ConversionAdvancedSection : UserControl
{
    public ConversionAdvancedSection()
    {
        InitializeComponent();
        BrowseDiskDefs.Click += (_, e) => BrowseDiskDefinitionsRequested?.Invoke(this, e);
    }
    public event RoutedEventHandler? InputChanged;
    public event RoutedEventHandler? BrowseDiskDefinitionsRequested;
    public CheckBox TracksEnabledCheckBox => TracksEnabled;
    public CheckBox DiskDefinitionsEnabled => DiskDefsEnabled;
    public TextBox DiskDefinitionsValue => DiskDefsValue;
    private void Input_Changed(object sender, RoutedEventArgs e) => InputChanged?.Invoke(sender, e);
}
