using System.Windows.Controls;

namespace GWGUI.App.Controls;

public partial class ReadImageSection : UserControl
{
    public ReadImageSection() => InitializeComponent();
    public RadioButton RawScpRadio => RawScp;
    public RadioButton KnownFormatRadio => KnownFormat;
    public Grid KnownFormatPanel => KnownFormatOptions;
    public ComboBox FamilyCombo => Family;
    public ComboBox FormatCombo => Format;
    public ComboBox ExtensionCombo => Extension;
}
