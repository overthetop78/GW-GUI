using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Controls;

public partial class ReadAdvancedSection : UserControl
{
    public ReadAdvancedSection()
    {
        InitializeComponent();
        BrowseDiskDefs.Click += (_, e) => BrowseDiskDefinitionsRequested?.Invoke(this, e);
    }

    public event RoutedEventHandler? InputChanged;
    public event RoutedEventHandler? FakeIndexChecked;
    public event RoutedEventHandler? HardSectorsChecked;
    public event RoutedEventHandler? DenselChecked;
    public event RoutedEventHandler? Tg43Checked;
    public event SelectionChangedEventHandler? SequenceKindChanged;
    public event RoutedEventHandler? BrowseDiskDefinitionsRequested;

    public CheckBox RevsEnabledCheckBox => RevsEnabled;
    public CheckBox AutoNumberCheckBox => AutoNumber;
    public ComboBox SequenceKindComboBox => SequenceKind;
    public ComboBox SequenceWidthComboBox => SequenceWidth;
    public TextBox SequenceValueTextBox => SequenceValue;
    public TextBlock NamePreviewTextBlock => NamePreview;
    public CheckBox DiskDefinitionsEnabled => DiskDefsEnabled;
    public TextBox DiskDefinitionsValue => DiskDefsValue;

    private void Input_Changed(object sender, RoutedEventArgs e) => InputChanged?.Invoke(sender, e);
    private void FakeIndex_Checked(object sender, RoutedEventArgs e) => FakeIndexChecked?.Invoke(sender, e);
    private void HardSectors_Checked(object sender, RoutedEventArgs e) => HardSectorsChecked?.Invoke(sender, e);
    private void Densel_Checked(object sender, RoutedEventArgs e) => DenselChecked?.Invoke(sender, e);
    private void Tg43_Checked(object sender, RoutedEventArgs e) => Tg43Checked?.Invoke(sender, e);
    private void SequenceKind_Changed(object sender, SelectionChangedEventArgs e) => SequenceKindChanged?.Invoke(sender, e);
}
