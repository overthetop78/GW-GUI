using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Controls;

public partial class ConversionOutputSection : UserControl
{
    public ConversionOutputSection() => InitializeComponent();
    public event RoutedEventHandler? ValueChanged;
    public TextBox OutputNameTextBox => OutputName;
    public CheckBox TagsCheckBox => Tags;
    public TextBlock SourceInformation => SourceInfo;
    private void Value_Changed(object sender, RoutedEventArgs e) => ValueChanged?.Invoke(sender, e);
}
