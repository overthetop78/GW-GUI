using GWGUI.MediaEngine.Formats;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.DiskImages;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Explorer;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.App.Views.Dialogs.Explorer;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Representations.Optical;
using GWGUI.MediaEngine.Representations.Sequential;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.App.Views.Controls.Explorer;

public partial class ExplorerSection
{
    private static IReadOnlyList<string> ReportedFormats(ExploredDiskImage document)
    {
        return document.FormatsDetectes
            .Select(format => format.FormatId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private string DetectedFormatsSummary()
    {
        if (_detectedFormatIds.Contains(TapeImageFormatIds.AtariCas, StringComparer.OrdinalIgnoreCase))
            return LocExtension.Get("Explorer.DetectedFormats", $"{LocExtension.Get("System.atari-8bit")} (Atari CAS)");

        var catalog = new DiskClassificationCatalog(_formats);
        var recognized = _detectedFormatIds.Select(id => catalog.ResolveFormat(id))
            .Where(format => format is not null).Cast<DiskFormat>()
            .DistinctBy(format => format.Id, StringComparer.OrdinalIgnoreCase)
            .Select(format => $"{format.Family} ({format.DisplayName})")
            .ToArray();
        var value = recognized.Length == 0
            ? ControlVisualConstants.EmptyValue
            : string.Join(ControlVisualConstants.DetailSeparator, recognized);
        return LocExtension.Get("Explorer.DetectedFormats", value);
    }

    private void UpdateDetectedFormatSelector(string? selectedId)
    {
        var catalog = new DiskClassificationCatalog(_formats);
        var choices = _detectedFormatIds
            .Select(id => catalog.ResolveFormat(id))
            .Where(format => format is not null)
            .Cast<DiskFormat>()
            .DistinctBy(format => format.Id, StringComparer.OrdinalIgnoreCase)
            .Select(format => new ExplorerFormatChoice(format.Id,
                $"{format.Family}{ControlVisualConstants.DetailSeparator}{format.DisplayName}"))
            .ToArray();
        _updatingDetectedFormatSelector = true;
        DetectedFormatSelector.ItemsSource = choices;
        DetectedFormatSelector.SelectedItem = choices.FirstOrDefault(choice =>
            string.Equals(choice.Id, selectedId, StringComparison.OrdinalIgnoreCase)) ?? choices.FirstOrDefault();
        var selectorVisible = choices.Length > 1;
        DetectedFormatSelector.Visibility = selectorVisible ? Visibility.Visible : Visibility.Collapsed;
        DetectedFormatLabel.Visibility = selectorVisible ? Visibility.Visible : Visibility.Collapsed;
        _updatingDetectedFormatSelector = false;
    }

    private void DetectedFormatSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingDetectedFormatSelector || DetectedFormatSelector.SelectedItem is not ExplorerFormatChoice { Id: { } formatId }) return;
        Classification.ApplyDetection(formatId, null, _detectedFormatIds);
        FormatChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ManualOptionsButton_Click(object sender, RoutedEventArgs e) =>
        ManualOptionsPopup.IsOpen = !ManualOptionsPopup.IsOpen;

    private CustomPopupPlacement[] ManualOptionsPopup_Place(Size popupSize, Size targetSize, Point offset) =>
        [new(new Point(targetSize.Width - popupSize.Width, targetSize.Height), PopupPrimaryAxis.Horizontal)];

    private string CurrentSystem(ExploredDiskImage document)
    {
        var format = new DiskClassificationCatalog(_formats).ResolveFormat(document.PrimaryFormatId);
        return format?.Family
            ?? Classification.SelectedMachine
            ?? ExplorerMetadataPresenter.Systems(document.Metadata);
    }

    private string CurrentSystem(ExploredMediaImage document)
    {
        if (document.Document.FormatId.Equals(TapeImageFormatIds.AtariCas, StringComparison.OrdinalIgnoreCase)
            && document.Document.Metadata.TryGetValue(ExplorerMediaMetadataConstants.SystemId, out var cassetteSystemId)
            && cassetteSystemId == DiskSystemIds.Atari8Bit)
            return LocExtension.Get("System.atari-8bit");
        if (Classification.SelectedMachine is { } selectedMachine) return selectedMachine;
        var format = new DiskClassificationCatalog(_formats).ResolveFormat(document.Document.FormatId);
        return format?.Family
            ?? (document.Document.Metadata.TryGetValue(ExplorerMediaMetadataConstants.SystemId, out var systemId)
                && systemId == DiskSystemIds.Atari8Bit ? LocExtension.Get("System.atari-8bit") : null)
            ?? document.Document.MediaKind.ToString();
    }

    private string TapeDescription(ExploredMediaImage document)
    {
        var cassette = LocExtension.Get("Explorer.Cassette");
        var system = CurrentSystem(document);
        if (!string.IsNullOrWhiteSpace(system)
            && !system.Equals(document.Document.MediaKind.ToString(), StringComparison.OrdinalIgnoreCase))
            return $"{cassette} {system}";
        var format = FormatForIdentity(document.Document.FormatId);
        return format is null ? cassette : $"{cassette} {format.DisplayName}";
    }

    private void ResetSummaryLabels()
    {
        VolumeLabel.Text = LocExtension.Get("Explorer.Volume");
        FileSystemLabel.Text = LocExtension.Get("Explorer.FileSystem");
        CapacityLabel.Text = LocExtension.Get("Explorer.Capacity");
        FreeLabel.Text = LocExtension.Get("Explorer.Free");
        EntryCountLabel.Text = LocExtension.Get("Explorer.Entries");
    }

    private Brush BrushFor(bool synthetic)
    {
        var resourceKey = synthetic
            ? ControlVisualConstants.SyntheticNameBrushResource
            : ControlVisualConstants.TextBrushResource;
        return TryFindResource(resourceKey) as Brush ?? SystemColors.WindowTextBrush;
    }

}
