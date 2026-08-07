using System.Windows.Controls;

namespace GWGUI.App.Controls;

public partial class VisualizerHeaderSection : UserControl
{
    public VisualizerHeaderSection() => InitializeComponent();
    public TextBlock FileNameText => FileName;
    public TextBlock SummaryText => Summary;
    public ComboBox DecoderCombo => Decoder;
    public CheckBox LinkZoomCheckBox => LinkZoom;
    public Button ResetButton => Reset;
    public Button OpenButton => Open;
}
