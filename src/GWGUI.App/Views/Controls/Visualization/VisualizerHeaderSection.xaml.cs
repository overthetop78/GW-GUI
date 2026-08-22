using GWGUI.Domain.Formats;
using GWGUI.App.Views.Controls.Common;
using System.Windows.Controls;


namespace GWGUI.App.Views.Controls.Visualization;

public partial class VisualizerHeaderSection : UserControl
{
    public VisualizerHeaderSection() => InitializeComponent();
    public TextBlock FileNameText => FileName;
    public TextBlock SummaryText => Summary;
    public ComboBox DecoderCombo => Decoder;
    public CheckBox LinkZoomCheckBox => LinkZoom;
    public Button ResetButton => Reset;
    public Button OpenButton => Open;
    public DiskClassificationSelector ClassificationSelector => Classification;
    public void SetFormats(IEnumerable<DiskFormat> formats) => Classification.SetCatalog(formats);
    public void ApplyDetection(string? formatId, string? protectionId) => Classification.ApplyDetection(formatId, protectionId);
    public void ApplyDetection(string? formatId, string? protectionId, IEnumerable<string> detectedFormatIds) => Classification.ApplyDetection(formatId, protectionId, detectedFormatIds);
}
