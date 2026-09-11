using GWGUI.Domain.Formats;
using GWGUI.App.Views.Controls.Common;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Visualization;
using System.IO;
using System.Windows;
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

    public void DisplayDocument(MediaImageDocument document, MediaVisualizationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(descriptor);

        FileName.Text = Path.GetFileName(document.Source.PrimaryPath);
        Format.Text = document.FormatId;
        MediaKind.Text = document.MediaKind.ToString();
        Representation.Text = document.Representation.RepresentationKind.ToString();
        MediaBadges.Visibility = Visibility.Visible;

        LinkZoom.Visibility = descriptor.Surfaces.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        Classification.Visibility = document.MediaKind == GWGUI.Domain.Enums.MediaKind.Floppy
                                    && descriptor.RepresentationKind == MediaRepresentationKind.Flux
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
