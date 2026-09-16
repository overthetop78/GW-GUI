using GWGUI.Domain.Formats;
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
    private DiskFormat? FormatForIdentity(string? formatId)
    {
        if (string.IsNullOrWhiteSpace(formatId)) return null;
        return _formats.FirstOrDefault(format => string.Equals(format.Id, formatId, StringComparison.OrdinalIgnoreCase))
            ?? BuiltInFormats.FirstOrDefault(format => string.Equals(format.Id, formatId, StringComparison.OrdinalIgnoreCase));
    }

    private void ConfigureSessions(int? preferredSession)
    {
        _updatingSessionSelector = true;
        if (_mediaDocument?.Document.Representation is not OpticalMediaImageRepresentation { Tracks: { } tracks })
        {
            SessionSelector.ItemsSource = null;
            SessionSelector.Visibility = Visibility.Collapsed;
            SessionControl.Visibility = Visibility.Collapsed;
            _updatingSessionSelector = false;
            return;
        }
        var choices = tracks.Select(track => track.SessionNumber)
            .Distinct()
            .Order()
            .Select(number => new ExplorerOpticalSessionChoice(
                number,
                $"{LocExtension.Get("Explorer.Session")} {number}"))
            .ToArray();
        SessionSelector.ItemsSource = choices;
        SessionControl.Visibility = choices.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        SessionSelector.Visibility = choices.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        SessionSelector.SelectedItem = choices.FirstOrDefault(choice => choice.SessionNumber == preferredSession)
            ?? choices.FirstOrDefault();
        _updatingSessionSelector = false;
    }

    private void ConfigureVolumesAndAudio(ExploredMediaVolume? preferredVolume)
    {
        if (_mediaDocument is null) return;
        var session = (SessionSelector.SelectedItem as ExplorerOpticalSessionChoice)?.SessionNumber;
        var volumes = _mediaDocument.Volumes
            .Where(volume => session is null || volume.Descriptor.SessionNumber is null || volume.Descriptor.SessionNumber == session)
            .ToArray();
        var choices = volumes
            .Select((volume, index) => new ExplorerMediaVolumeChoice(volume, VolumeChoiceName(volume, index)))
            .ToArray();
        var selectedVolume = preferredVolume is not null && volumes.Contains(preferredVolume)
            ? preferredVolume
            : volumes.FirstOrDefault(volume => volume.FileSystem is not null) ?? volumes.FirstOrDefault();
        _updatingVolumeSelector = true;
        VolumeSelector.ItemsSource = choices;
        VolumeSelector.Visibility = choices.Length > 1 ? Visibility.Visible : Visibility.Collapsed;
        VolumeSelector.SelectedItem = choices.FirstOrDefault(choice => ReferenceEquals(choice.Volume, selectedVolume));
        _updatingVolumeSelector = false;

        var audioTracks = (_mediaDocument.Document.Representation as OpticalMediaImageRepresentation)?.Tracks?
            .Where(track => track.IsAudio && (session is null || track.SessionNumber == session))
            .Select(track => new ExplorerOpticalTrackChoice(
                track,
                $"{LocExtension.Get("Explorer.Track")} {track.TrackNumber} \u00b7 {LocExtension.Get("Explorer.Audio")}"))
            .ToArray() ?? [];
        AudioTrackList.ItemsSource = audioTracks;
        AudioTrackList.SelectedItem = null;
        OpticalTracksSection.Visibility = audioTracks.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        DisplayMediaVolume(selectedVolume);
    }

    private void DisplayMediaVolume(ExploredMediaVolume? exploredVolume)
    {
        if (_mediaDocument is null) return;
        _mediaVolume = exploredVolume;

        var volume = _mediaVolume?.FileSystem;
        var isTape = _mediaDocument.Document.MediaKind == GWGUI.Domain.Enums.MediaKind.Tape;
        if (isTape)
        {
            VolumeLabel.Text = LocExtension.Get("Explorer.Volume");
            FileSystemLabel.Text = LocExtension.Get("Explorer.Format");
            CapacityLabel.Text = LocExtension.Get("Explorer.Size");
            FreeLabel.Text = LocExtension.Get("Visual.DurationLabel");
            EntryCountLabel.Text = LocExtension.Get("Explorer.Entries");
        }
        else
        {
            ResetSummaryLabels();
        }
        var descriptorName = _mediaVolume?.Descriptor.Name;
        var syntheticName = string.IsNullOrWhiteSpace(volume?.Name) && string.IsNullOrWhiteSpace(descriptorName);
        var volumeName = !string.IsNullOrWhiteSpace(volume?.Name)
            ? volume.Name
            : !string.IsNullOrWhiteSpace(descriptorName)
                ? descriptorName
                : $"({LocExtension.Get("Explorer.Unnamed")})";
        VolumeNameText.Foreground = BrushFor(syntheticName);
        VolumeNameText.Text = volumeName;
        SystemText.Text = CurrentSystem(_mediaDocument);
        ProtectionText.Text = LocExtension.Get("Explorer.Metadata.None");
        FileSystemText.Text = isTape
            ? TapeDescription(_mediaDocument)
            : volume?.FileSystemId ?? ControlVisualConstants.EmptyValue;
        var capacity = volume?.Capacity ?? _mediaVolume?.Descriptor.Length;
        CapacityText.Text = capacity.HasValue ? StorageSizeFormatter.FormatBytes(capacity.Value) : ControlVisualConstants.EmptyValue;
        FreeText.Text = isTape
            ? (_mediaDocument.Document.Representation as SequentialMediaImageRepresentation)?.Duration?.ToString("g") ?? ControlVisualConstants.EmptyValue
            : volume?.FreeSpaceKnown == true
                ? StorageSizeFormatter.FormatBytes(volume.FreeBytes)
                : ControlVisualConstants.EmptyValue;
        _rootEntries = volume?.Entries ?? [];
        EntryCountText.Text = CountEntries(_rootEntries).ToString();
        _rootFolder = new ExplorerFolderItem(volumeName, null, 0, _rootEntries, syntheticName) { IsExpanded = true };
        RefreshVisibleFolders(_rootFolder);
        FolderList.SelectedItem = _rootFolder;
        ShowContents(_rootEntries);

        if (_mediaVolume is not null)
            DetailsPanel.ShowMedia(_mediaDocument, _mediaVolume, CurrentSystem(_mediaDocument));
        else
            DetailsPanel.Clear();
        var issues = _mediaVolume is null ? _mediaDocument.Diagnostics : ExplorerIssueBuilder.Build(_mediaDocument, _mediaVolume);
        WarningsButton.Visibility = issues.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        WarningsText.Text = $"{LocExtension.Get("Explorer.Warnings")} : {issues.Count}";
    }

    private static string VolumeChoiceName(ExploredMediaVolume volume, int index)
    {
        var name = volume.FileSystem?.Name ?? volume.Descriptor.Name;
        var prefix = string.IsNullOrWhiteSpace(name) ? $"{LocExtension.Get("Explorer.Volume")} {index + 1}" : name;
        var partition = volume.Descriptor.PartitionNumber is { } number
            ? $"{ControlVisualConstants.DetailSeparator}#{number}"
            : string.Empty;
        var track = volume.Descriptor.TrackNumber is { } trackNumber
            ? $" \u00b7 {LocExtension.Get("Explorer.Track")} {trackNumber}"
            : string.Empty;
        return $"{prefix}{partition}{track} \u00b7 {StorageSizeFormatter.FormatBytes(volume.Descriptor.Length)}";
    }

    private void SessionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingSessionSelector) return;
        ConfigureVolumesAndAudio(null);
    }

    private void VolumeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingVolumeSelector || VolumeSelector.SelectedItem is not ExplorerMediaVolumeChoice choice) return;
        AudioTrackList.SelectedItem = null;
        DisplayMediaVolume(choice.Volume);
    }

    private void AudioTrackList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AudioTrackList.SelectedItem is ExplorerOpticalTrackChoice choice)
            DetailsPanel.ShowOpticalTrack(choice.Track);
        else if (_mediaDocument is not null && _mediaVolume is not null)
            DetailsPanel.ShowMedia(_mediaDocument, _mediaVolume, CurrentSystem(_mediaDocument));
    }

}
