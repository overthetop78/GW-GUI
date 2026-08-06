using System.Windows.Controls;

namespace GWGUI.App.Controls;

public partial class WriteFormatSection : UserControl
{
    public WriteFormatSection() => InitializeComponent();
    public TextBlock DetectionText => Detection;
    public ComboBox FormatCombo => Formats;
    public Button ModifyButton => Modify;
    public Button VisualizeTracksButton => VisualizeTracks;
}
