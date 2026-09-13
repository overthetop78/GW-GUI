using GWGUI.Domain.Formats;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Explorer;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.App.Views.Dialogs.Explorer;
using GWGUI.App.Views.Controls.Common;
using System.Collections.ObjectModel;
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

public partial class ExplorerSection : UserControl
{
    private static readonly IReadOnlyList<DiskFormat> BuiltInFormats = new BuiltInImageFormatCatalog().Formats;
    private ExplorerFolderItem? _rootFolder;
    private ExploredDiskImage? _document;
    private ExploredMediaImage? _mediaDocument;
    private ExploredMediaVolume? _mediaVolume;
    private IReadOnlyList<FileSystemEntry> _rootEntries = [];
    private IReadOnlyList<DiskFormat> _formats = [];
    private IReadOnlyList<string> _detectedFormatIds = [];
    private string? _automaticDetectedFormatId;
    private readonly ObservableCollection<ExplorerFolderItem> _visibleFolders = [];
    private bool _updatingVolumeSelector;
    private bool _updatingSessionSelector;
    private bool _updatingDetectedFormatSelector;
    public ExplorerSection()
    {
        InitializeComponent();
        FolderList.ItemsSource = _visibleFolders;
        SetFormats([], null);
        Classification.ValueChanged += (_, _) =>
        {
            AutomaticDetection.IsChecked = false;
            Classification.SetAutomaticDetection(false);
            FormatChanged?.Invoke(this, EventArgs.Empty);
        };
        OpenButton.Click += (_, e) => OpenRequested?.Invoke(this, e);
        EmptyOpenButton.Click += (_, e) => OpenRequested?.Invoke(this, e);
        ReadDiskButton.Click += (_, e) => ReadDiskRequested?.Invoke(this, e);
        SessionSelector.SelectionChanged += SessionSelector_SelectionChanged;
        AudioTrackList.SelectionChanged += AudioTrackList_SelectionChanged;
    }

    public event RoutedEventHandler? OpenRequested;
    public event RoutedEventHandler? ReadDiskRequested;
    public event EventHandler? FormatChanged;
    public Button OpenImageButton => OpenButton;
    public CardSection HeaderCardControl => HeaderCardRoot;
    public DiskClassificationSelector ClassificationSelector => Classification;
    public IReadOnlyList<ExplorerFormatChoice> FormatChoices =>
        [new(null, LocExtension.Get("Explorer.Automatic")), .. _formats.Select(format => new ExplorerFormatChoice(format.Id, format.DisplayName))];
    public void SetReadDiskRunning(bool running) => ReadDiskButton.Content = LocExtension.Get(running ? "Common.Stop" : "Explorer.ReadDisk");
    public string? SelectedFormatId => AutomaticDetection.IsChecked == true
        ? (DetectedFormatSelector.SelectedItem as ExplorerFormatChoice)?.Id ?? Classification.SelectedFormatId
        : Classification.SelectedFormatId;
    public string? FormatIdForNewImage => AutomaticDetection.IsChecked == true ? null : SelectedFormatId;

    public void SelectDetectedFormat(string formatId)
    {
        if (DetectedFormatSelector.ItemsSource is not IEnumerable<ExplorerFormatChoice> choices) return;
        var choice = choices.FirstOrDefault(item => string.Equals(item.Id, formatId, StringComparison.OrdinalIgnoreCase));
        if (choice is null) return;
        _updatingDetectedFormatSelector = true;
        DetectedFormatSelector.SelectedItem = choice;
        Classification.ApplyDetection(formatId, null, _detectedFormatIds);
        _updatingDetectedFormatSelector = false;
    }

    private void AutomaticDetection_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized)
            return;

        var enabled = AutomaticDetection.IsChecked == true;
        Classification.SetAutomaticDetection(enabled);
        if (!enabled) return;

        var formatId = _automaticDetectedFormatId
            ?? _document?.PrimaryFormatId
            ?? _mediaDocument?.Document.FormatId;
        if (string.IsNullOrWhiteSpace(formatId)) return;

        SelectDetectedFormat(formatId);
        FormatChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetFormats(IEnumerable<DiskFormat> formats, string? selectedId)
    {
        var hadSelection = Classification.SelectedFormatId is not null;
        _formats = formats.ToArray();
        Classification.SetCatalog(_formats);
        if (!hadSelection && selectedId is not null) Classification.ApplyDetection(selectedId, null);
    }

    public void SetLoading(bool loading)
    {
        LoadingOverlay.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
        if (loading)
            SetLoadingProgress(LocExtension.Get("Explorer.Loading"), string.Empty, 0);
    }

    public void SetLoadingProgress(string stage, string detail, double value)
    {
        LoadingCard.SetProgress(stage, detail, value);
    }

    internal string LoadingStage => LoadingCard.Stage;
    internal string LoadingDetail => LoadingCard.Detail;
    internal double LoadingValue => LoadingCard.Value;
    internal string LoadingPercent => LoadingCard.Percent;

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
        VolumeNameText.Text = FileSystemText.Text = CapacityText.Text = FreeText.Text = EntryCountText.Text = "—";
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
            GWGUI.Domain.Enums.MediaKind.Floppy,
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
        var partition = volume.Descriptor.PartitionNumber is { } number ? $" · #{number}" : string.Empty;
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
            return LocExtension.Get("Explorer.DetectedFormats", "Atari 8-bit (Atari CAS)");

        var catalog = new DiskClassificationCatalog(_formats);
        var recognized = _detectedFormatIds.Select(id => catalog.ResolveFormat(id))
            .Where(format => format is not null).Cast<DiskFormat>()
            .DistinctBy(format => format.Id, StringComparer.OrdinalIgnoreCase)
            .Select(format => $"{format.Family} ({format.DisplayName})")
            .ToArray();
        var value = recognized.Length == 0 ? "\u2014" : string.Join("  \u00b7  ", recognized);
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
            .Select(format => new ExplorerFormatChoice(format.Id, $"{format.Family} · {format.DisplayName}"))
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
            && document.Document.Metadata.TryGetValue("systemId", out var cassetteSystemId)
            && cassetteSystemId == DiskSystemIds.Atari8Bit)
            return "Atari 8-bit";
        if (Classification.SelectedMachine is { } selectedMachine) return selectedMachine;
        var format = new DiskClassificationCatalog(_formats).ResolveFormat(document.Document.FormatId);
        return format?.Family
            ?? (document.Document.Metadata.TryGetValue("systemId", out var systemId) && systemId == DiskSystemIds.Atari8Bit ? "Atari 8-bit" : null)
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
        var resourceKey = synthetic ? "SyntheticNameBrush" : "TextBrush";
        return TryFindResource(resourceKey) as Brush ?? SystemColors.WindowTextBrush;
    }

    public static int CountEntries(IEnumerable<FileSystemEntry> entries) => ExplorerIssueBuilder.CountEntries(entries);

    private void RefreshVisibleFolders(ExplorerFolderItem? selected = null)
    {
        if (_rootFolder is null) return;
        _visibleFolders.Clear();
        foreach (var item in ExplorerTreeNavigator.Flatten(_rootFolder)) _visibleFolders.Add(item);
        if (selected is not null) FolderList.SelectedItem = selected;
    }

    private void ShowContents(IEnumerable<FileSystemEntry> entries)
    {
        var family = _document is not null
            ? ExplorerFileIconClassifier.FamilyFor(_document)
            : _mediaDocument is not null
                ? ExplorerFileIconClassifier.FamilyFor(_mediaDocument.Document.FormatId, _mediaVolume?.FileSystem?.FileSystemId)
                : ExplorerFileSystemFamily.Unknown;
        ContentsList.ItemsSource = entries
            .OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory)
            .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(entry => new ExplorerContentItem(entry, family)).ToArray();
        ContentsList.SelectedItem = null;
        if (_document is not null) DetailsPanel.ShowDisk(_document, CurrentSystem(_document));
        else if (_mediaDocument is not null && _mediaVolume is not null)
            DetailsPanel.ShowMedia(_mediaDocument, _mediaVolume, CurrentSystem(_mediaDocument));
    }

    private void ContentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_document is null && _mediaDocument is null) return;
        if (ContentsList.SelectedItem is ExplorerContentItem item)
        {
            if (_document is not null) DetailsPanel.ShowItem(_document, item);
            else DetailsPanel.ShowItem(item);
        }
        else if (_document is not null) DetailsPanel.ShowDisk(_document, CurrentSystem(_document));
        else if (_mediaDocument is not null && _mediaVolume is not null)
            DetailsPanel.ShowMedia(_mediaDocument, _mediaVolume, CurrentSystem(_mediaDocument));
    }

    private void FolderToggle_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ExplorerFolderItem item } || item.Children.Count == 0) return;
        item.IsExpanded = !item.IsExpanded;
        RefreshVisibleFolders(item);
        ShowContents(item.Entry?.Children ?? _rootEntries);
        e.Handled = true;
    }

    private void FolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FolderList.SelectedItem is ExplorerFolderItem item) ShowContents(item.Entry?.Children ?? _rootEntries);
    }

    private void FolderList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var element = e.OriginalSource as DependencyObject;
        while (element is not null && element is not ListBoxItem)
            element = VisualTreeHelper.GetParent(element);
        if (element is not ListBoxItem { DataContext: ExplorerFolderItem item }) return;
        ContentsList.SelectedItem = null;
        ShowContents(item.Entry?.Children ?? _rootEntries);
    }

    private void ContentsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ContentsList.SelectedItem is not ExplorerContentItem { Entry.Kind: FileSystemEntryKind.Directory } selected || _rootFolder is null) return;
        var folder = ExplorerTreeNavigator.Find(_rootFolder, selected.Entry);
        if (folder is null) return;
        ExplorerTreeNavigator.ExpandPathTo(_rootFolder, folder);
        RefreshVisibleFolders(folder);
        ShowContents(folder.Entry!.Children);
    }

    private void WarningsButton_Click(object sender, RoutedEventArgs e)
    {
        var issues = _document is not null
            ? BuildIssues(_document)
            : _mediaDocument is not null && _mediaVolume is not null
                ? ExplorerIssueBuilder.Build(_mediaDocument, _mediaVolume)
                : [];
        if (issues.Count == 0) return;
        new ExplorerIssuesWindow(issues) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    public static IReadOnlyList<string> BuildIssues(ExploredDiskImage document) => ExplorerIssueBuilder.Build(document);
}
