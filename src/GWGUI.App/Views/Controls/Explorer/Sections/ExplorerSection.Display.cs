using GWGUI.MediaEngine.Formats;
using GWGUI.App.Constants.Controls.Visual;
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
    public void Clear(string? path = null, bool newImage = true)
    {
        ManualOptionsPopup.IsOpen = false;
        if (newImage)
        {
            _detectedFormatIds = [];
            _automaticDetectedFormatId = null;
            UpdateDetectedFormatSelector(null);
        }
        PathText.Text = path ?? string.Empty;
        DocumentIdentity.Display(path ?? string.Empty, string.Empty, null);
        VolumeNameText.Foreground = BrushFor(false);
        VolumeNameText.Text = FileSystemText.Text = CapacityText.Text = FreeText.Text = EntryCountText.Text =
            ControlVisualConstants.EmptyValue;
        SystemText.Text = ProtectionText.Text = "\u2014";
        _rootFolder = null;
        _document = null;
        _mediaDocument = null;
        _mediaVolume = null;
        _updatingVolumeSelector = true;
        VolumeSelector.ItemsSource = null;
        VolumeSelector.Visibility = Visibility.Collapsed;
        _updatingVolumeSelector = false;
        _updatingSessionSelector = true;
        SessionSelector.ItemsSource = null;
        SessionSelector.Visibility = Visibility.Collapsed;
        _updatingSessionSelector = false;
        AudioTrackList.ItemsSource = null;
        OpticalTracksSection.Visibility = Visibility.Collapsed;
        _rootEntries = [];
        _visibleFolders.Clear();
        ContentsList.ItemsSource = null;
        WarningsButton.Visibility = Visibility.Collapsed;
        DetailsPanel.Clear();
        ExplorerEmptyState.Visibility = string.IsNullOrWhiteSpace(path) ? Visibility.Visible : Visibility.Collapsed;
    }

    public void Display(ExploredDiskImage document)
    {
        ExplorerEmptyState.Visibility = Visibility.Collapsed;
        _document = document;
        _mediaDocument = null;
        _mediaVolume = null;
        _updatingSessionSelector = true;
        SessionSelector.ItemsSource = null;
        SessionSelector.Visibility = Visibility.Collapsed;
        _updatingSessionSelector = false;
        AudioTrackList.ItemsSource = null;
        OpticalTracksSection.Visibility = Visibility.Collapsed;
        _updatingVolumeSelector = true;
        VolumeSelector.ItemsSource = null;
        VolumeSelector.Visibility = Visibility.Collapsed;
        _updatingVolumeSelector = false;
        ResetSummaryLabels();
        PathText.Text = document.SourcePath;
        var reportedFormatIds = ReportedFormats(document);
        if (_automaticDetectedFormatId is null || AutomaticDetection.IsChecked == true)
        {
            _automaticDetectedFormatId = document.PrimaryFormatId;
            _detectedFormatIds = reportedFormatIds;
        }
        else
        {
            _detectedFormatIds = _detectedFormatIds
                .Concat(reportedFormatIds)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        UpdateDetectedFormatSelector(document.PrimaryFormatId);
        var detectedSummary = DetectedFormatsSummary();
        DocumentIdentity.Display(
            document.SourcePath,
            detectedSummary,
            GWGUI.MediaEngine.Enums.MediaKind.Floppy,
            FormatForIdentity(document.PrimaryFormatId));
        Classification.SetAutomaticDetection(AutomaticDetection.IsChecked == true);
        if (AutomaticDetection.IsChecked == true)
        {
            Classification.ApplyDetection(document.PrimaryFormatId, document.Metadata.ProtectionId, _detectedFormatIds);
        }
        var volumeName = ExplorerDetailsPresenter.VolumeName(document);
        VolumeNameText.Text = volumeName.Text;
        VolumeNameText.Foreground = BrushFor(volumeName.IsSynthetic);
        var currentSystem = CurrentSystem(document);
        SystemText.Text = currentSystem;
        ProtectionText.Text = ExplorerMetadataPresenter.Protection(document.Metadata);
        FileSystemText.Text = ExplorerDetailsPresenter.FileSystemText(document);
        CapacityText.Text = StorageSizeFormatter.FormatBytes(document.Volume.Capacity);
        FreeText.Text = document.FileSystemRecognized && document.Volume.FreeSpaceKnown ? StorageSizeFormatter.FormatBytes(document.Volume.FreeBytes) : ControlVisualConstants.EmptyValue;
        EntryCountText.Text = CountEntries(document.Volume.Entries).ToString();
        _rootEntries = document.Volume.Entries;
        _rootFolder = new ExplorerFolderItem(volumeName.Text, null, 0, _rootEntries, volumeName.IsSynthetic) { IsExpanded = true };
        RefreshVisibleFolders(_rootFolder);
        FolderList.SelectedItem = _rootFolder;
        ShowContents(_rootEntries);
        DetailsPanel.ShowDisk(document, currentSystem);
        var warningCount = BuildIssues(document).Count;
        WarningsButton.Visibility = warningCount == 0 ? Visibility.Collapsed : Visibility.Visible;
        WarningsText.Text = $"{LocExtension.Get("Explorer.Warnings")} : {warningCount}";
    }

    public void Display(ExploredMediaImage document)
    {
        ArgumentNullException.ThrowIfNull(document);
        ExplorerEmptyState.Visibility = Visibility.Collapsed;
        _document = null;
        _mediaDocument = document;
        PathText.Text = document.Document.Source.PrimaryPath;
        if (_automaticDetectedFormatId is null || AutomaticDetection.IsChecked == true)
        {
            _automaticDetectedFormatId = document.Document.FormatId;
            _detectedFormatIds = [document.Document.FormatId];
        }
        UpdateDetectedFormatSelector(document.Document.FormatId);
        var detectedSummary = DetectedFormatsSummary();
        DocumentIdentity.Display(
            document.Document.Source.PrimaryPath,
            detectedSummary,
            document.Document.MediaKind,
            FormatForIdentity(document.Document.FormatId));
        Classification.SetAutomaticDetection(AutomaticDetection.IsChecked == true);
        if (AutomaticDetection.IsChecked == true)
            Classification.ApplyDetection(document.Document.FormatId, null, _detectedFormatIds);

        var selectedVolume = document.Volumes.FirstOrDefault(volume => volume.FileSystem is not null)
            ?? document.Volumes.FirstOrDefault();
        ConfigureSessions(selectedVolume?.Descriptor.SessionNumber);
        ConfigureVolumesAndAudio(selectedVolume);
    }

}
