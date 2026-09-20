using GWGUI.MediaEngine.Formats;
using GWGUI.App.Views.Controls.Common;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Visualization;
using GWGUI.App.Localization.Extensions;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;


namespace GWGUI.App.Views.Controls.Visualization;

public partial class VisualizerHeaderSection : UserControl
{
    private IReadOnlyList<DiskFormat> formats = [];
    private bool updatingRepresentationChoice;
    private bool updatingRevolutionChoice;
    private string? selectedClassificationFormatId;
    private string detectedFormatsSummary = string.Empty;
    public VisualizerHeaderSection()
    {
        InitializeComponent();
        Classification.ValueChanged += Classification_ValueChanged;
    }
    public event EventHandler<string?>? RepresentationChoiceChanged;
    public event EventHandler<string?>? ClassificationFormatChanged;
    public event Action<int?>? RevolutionChoiceChanged;
    public TextBlock FileNameText => DocumentIdentity.FileNameText;
    public TextBlock SummaryText => DocumentIdentity.SummaryText;
    public ComboBox DecoderCombo => Decoder;
    public ComboBox RevolutionCombo => RevolutionChoice;
    public CheckBox LinkZoomCheckBox => LinkZoom;
    public Button ResetButton => Reset;
    public Button OpenButton => Open;
    public string MediaIconKind => DocumentIdentity.MediaIconKind;
    public CardSection HeaderCardControl => HeaderCardRoot;
    public MediaDocumentIdentity DocumentIdentityControl => DocumentIdentity;
    public DiskClassificationSelector ClassificationSelector => Classification;
    public string? SelectedRepresentationId => (RepresentationChoice.SelectedItem as ExplorerFormatChoice)?.Id;

    public void SelectRepresentation(string? formatId)
    {
        if (RepresentationChoice.ItemsSource is not IEnumerable<ExplorerFormatChoice> choices) return;
        var choice = choices.FirstOrDefault(item => string.Equals(item.Id, formatId, StringComparison.OrdinalIgnoreCase));
        if (choice is null) return;
        updatingRepresentationChoice = true;
        RepresentationChoice.SelectedItem = choice;
        updatingRepresentationChoice = false;
    }
    public void SetFormats(IEnumerable<DiskFormat> formats)
    {
        this.formats = formats.ToArray();
        Classification.SetCatalog(this.formats);
    }
    public void ApplyDetection(string? formatId, string? protectionId)
    {
        Classification.ApplyDetection(formatId, protectionId);
        selectedClassificationFormatId = Classification.SelectedFormatId;
        detectedFormatsSummary = BuildDetectedFormatsSummary(
            string.IsNullOrWhiteSpace(formatId) ? [] : [formatId]);
    }
    public void ApplyDetection(string? formatId, string? protectionId, IEnumerable<string> detectedFormatIds, bool includeFlux = true)
    {
        var ids = detectedFormatIds.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        Classification.ApplyDetection(formatId, protectionId, ids);
        selectedClassificationFormatId = Classification.SelectedFormatId;
        detectedFormatsSummary = BuildDetectedFormatsSummary(ids);
        var catalog = new DiskClassificationCatalog(formats);
        var detectedChoices = ids.Select(catalog.ResolveFormat)
                .Where(format => format is not null)
                .Cast<DiskFormat>()
                .DistinctBy(format => format.Id, StringComparer.OrdinalIgnoreCase)
                .Select(format => new ExplorerFormatChoice(format.Id, $"{format.Family} · {format.DisplayName}"));
        var choices = (includeFlux
                ? new[] { new ExplorerFormatChoice(null, "Flux") }.Concat(detectedChoices)
                : detectedChoices)
            .ToArray();
        updatingRepresentationChoice = true;
        RepresentationChoice.ItemsSource = choices;
        RepresentationChoice.SelectedIndex = 0;
        RepresentationChoice.Visibility = choices.Length > 1 ? Visibility.Visible : Visibility.Collapsed;
        updatingRepresentationChoice = false;
    }

    private void RepresentationChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (updatingRepresentationChoice || RepresentationChoice.SelectedItem is not ExplorerFormatChoice choice) return;
        RepresentationChoiceChanged?.Invoke(this, choice.Id);
    }

    private void Classification_ValueChanged(object? sender, EventArgs e)
    {
        var formatId = Classification.SelectedFormatId;
        if (string.Equals(selectedClassificationFormatId, formatId, StringComparison.OrdinalIgnoreCase)) return;
        selectedClassificationFormatId = formatId;
        ClassificationFormatChanged?.Invoke(this, formatId);
    }

    public void ConfigureRevolutions(int count)
    {
        updatingRevolutionChoice = true;
        RevolutionChoice.ItemsSource = new[] { new RevolutionChoiceItem(null, LocExtension.Get("Visual.RevolutionSummary")) }
            .Concat(Enumerable.Range(1, Math.Max(0, count))
                .Select(number => new RevolutionChoiceItem(number - 1, LocExtension.Get("Visual.RevolutionChoice", number))))
            .ToArray();
        RevolutionChoice.SelectedIndex = 0;
        RevolutionControl.Visibility = Visibility.Visible;
        updatingRevolutionChoice = false;
    }

    public void ClearRevolutions()
    {
        updatingRevolutionChoice = true;
        RevolutionChoice.ItemsSource = null;
        RevolutionControl.Visibility = Visibility.Hidden;
        updatingRevolutionChoice = false;
    }

    private void RevolutionChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (updatingRevolutionChoice) return;
        RevolutionChoiceChanged?.Invoke((RevolutionChoice.SelectedItem as RevolutionChoiceItem)?.Index);
    }

    public void DisplayDocument(MediaImageDocument document, MediaVisualizationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(descriptor);

        DocumentIdentity.Display(
            document.Source.PrimaryPath,
            detectedFormatsSummary,
            document.MediaKind,
            formats.FirstOrDefault(format => string.Equals(format.Id, document.FormatId, StringComparison.OrdinalIgnoreCase)));

        LinkZoom.Visibility = descriptor.Surfaces.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        ManualOptionsButton.Visibility = document.MediaKind == GWGUI.MediaEngine.Enums.MediaKind.Floppy
            ? Visibility.Visible
            : Visibility.Collapsed;
        RevolutionControl.Visibility = descriptor.RepresentationKind == MediaRepresentationKind.Flux
                                       && RevolutionChoice.Items.Count > 0
            ? Visibility.Visible
            : Visibility.Hidden;
    }

    private void ManualOptionsButton_Click(object sender, RoutedEventArgs e) =>
        ManualOptionsPopup.IsOpen = !ManualOptionsPopup.IsOpen;

    private CustomPopupPlacement[] ManualOptionsPopup_Place(Size popupSize, Size targetSize, Point offset) =>
        [new(new Point(targetSize.Width - popupSize.Width, targetSize.Height), PopupPrimaryAxis.Horizontal)];

    private string BuildDetectedFormatsSummary(IEnumerable<string> formatIds)
    {
        var ids = formatIds.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (ids.Contains(GWGUI.MediaEngine.Constants.TapeImageFormatIds.AtariCas, StringComparer.OrdinalIgnoreCase))
            return LocExtension.Get("Explorer.DetectedFormats", "Atari 8-bit (Atari CAS)");

        var catalog = new DiskClassificationCatalog(formats);
        var detected = ids
            .Select(catalog.ResolveFormat)
            .Where(format => format is not null)
            .Cast<DiskFormat>()
            .DistinctBy(format => format.Id, StringComparer.OrdinalIgnoreCase)
            .Select(format => $"{format.Family} ({format.DisplayName})")
            .ToArray();
        var value = detected.Length == 0 ? "\u2014" : string.Join("  \u00b7  ", detected);
        return LocExtension.Get("Explorer.DetectedFormats", value);
    }

    private sealed record RevolutionChoiceItem(int? Index, string Label);
}
