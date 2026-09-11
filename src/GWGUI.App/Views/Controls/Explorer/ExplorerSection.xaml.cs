using GWGUI.Domain.Formats;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Explorer;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.App.Views.Dialogs.Explorer;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Exploration.Results;


namespace GWGUI.App.Views.Controls.Explorer;

public partial class ExplorerSection : UserControl
{
    private ExplorerFolderItem? _rootFolder;
    private ExploredDiskImage? _document;
    private ExploredMediaImage? _mediaDocument;
    private ExploredMediaVolume? _mediaVolume;
    private IReadOnlyList<FileSystemEntry> _rootEntries = [];
    private IReadOnlyList<DiskFormat> _formats = [];
    private IReadOnlyList<string> _detectedFormatIds = [];
    private readonly ObservableCollection<ExplorerFolderItem> _visibleFolders = [];
    private bool _applyDetectionOnDisplay;

    public ExplorerSection()
    {
        InitializeComponent();
        FolderList.ItemsSource = _visibleFolders;
        SetFormats([], null);
        Classification.ValueChanged += (_, _) => FormatChanged?.Invoke(this, EventArgs.Empty);
        OpenButton.Click += (_, e) => OpenRequested?.Invoke(this, e);
        ReadDiskButton.Click += (_, e) => ReadDiskRequested?.Invoke(this, e);
    }

    public event RoutedEventHandler? OpenRequested;
    public event RoutedEventHandler? ReadDiskRequested;
    public event EventHandler? FormatChanged;
    public Button OpenImageButton => OpenButton;
    public IReadOnlyList<ExplorerFormatChoice> FormatChoices =>
        [new(null, LocExtension.Get("Explorer.Automatic")), .. _formats.Select(format => new ExplorerFormatChoice(format.Id, format.DisplayName))];
    public void SetReadDiskRunning(bool running) => ReadDiskButton.Content = LocExtension.Get(running ? "Common.Stop" : "Explorer.ReadDisk");
    public string? SelectedFormatId => Classification.SelectedProtectionId ?? Classification.SelectedFormatId;
    public string? FormatIdForNewImage => AutomaticDetection.IsChecked == true ? null : SelectedFormatId;

    private void AutomaticDetection_Changed(object sender, RoutedEventArgs e)
    {
        var enabled = AutomaticDetection.IsChecked == true;
        Classification.SetAutomaticDetection(enabled);
        if (enabled && _document is not null)
            Classification.ApplyDetection(_document.PrimaryFormatId, _document.Metadata.ProtectionId, _detectedFormatIds);
        else if (enabled && _mediaDocument is not null)
            Classification.ApplyDetection(_mediaDocument.Document.FormatId, null, _detectedFormatIds);
    }

    public void SetFormats(IEnumerable<DiskFormat> formats, string? selectedId)
    {
        var hadSelection = Classification.SelectedFormatId is not null;
        _formats = formats.ToArray();
        Classification.SetCatalog(_formats);
        if (!hadSelection && selectedId is not null) Classification.ApplyDetection(selectedId, null);
    }

    public void SetLoading(bool loading) => LoadingOverlay.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;

    public void Clear(string? path = null, bool newImage = true)
    {
        _applyDetectionOnDisplay = newImage && AutomaticDetection.IsChecked == true;
        if (newImage) _detectedFormatIds = [];
        PathText.Text = path ?? string.Empty;
        DetectedFormatsText.Text = "\u2014";
        DetectedFormatsText.ToolTip = null;
        VolumeNameText.Foreground = BrushFor(false);
        VolumeNameText.Text = FileSystemText.Text = CapacityText.Text = FreeText.Text = EntryCountText.Text = "—";
        SystemText.Text = ProtectionText.Text = "\u2014";
        _rootFolder = null;
        _document = null;
        _mediaDocument = null;
        _mediaVolume = null;
        _rootEntries = [];
        _visibleFolders.Clear();
        ContentsList.ItemsSource = null;
        WarningsButton.Visibility = Visibility.Collapsed;
        DetailsPanel.Clear();
    }

    public void Display(ExploredDiskImage document)
    {
        _document = document;
        _mediaDocument = null;
        _mediaVolume = null;
        PathText.Text = document.SourcePath;
        _detectedFormatIds = _detectedFormatIds
            .Concat(ReportedFormats(document))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var detectedSummary = DetectedFormatsSummary();
        DetectedFormatsText.Text = detectedSummary;
        DetectedFormatsText.ToolTip = detectedSummary;
        Classification.SetAutomaticDetection(AutomaticDetection.IsChecked == true);
        if (_applyDetectionOnDisplay)
        {
            Classification.ApplyDetection(document.PrimaryFormatId, document.Metadata.ProtectionId, _detectedFormatIds);
        }

        _applyDetectionOnDisplay = false;
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
        _document = null;
        _mediaDocument = document;
        _mediaVolume = document.Volumes.FirstOrDefault(volume => volume.FileSystem is not null)
            ?? document.Volumes.FirstOrDefault();
        PathText.Text = document.Document.Source.PrimaryPath;
        _detectedFormatIds = _detectedFormatIds
            .Append(document.Document.FormatId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var detectedSummary = DetectedFormatsSummary();
        DetectedFormatsText.Text = detectedSummary;
        DetectedFormatsText.ToolTip = detectedSummary;
        Classification.SetAutomaticDetection(AutomaticDetection.IsChecked == true);
        if (_applyDetectionOnDisplay)
            Classification.ApplyDetection(document.Document.FormatId, null, _detectedFormatIds);
        _applyDetectionOnDisplay = false;

        var volume = _mediaVolume?.FileSystem;
        var syntheticName = string.IsNullOrWhiteSpace(volume?.Name);
        var volumeName = syntheticName ? $"({LocExtension.Get("Explorer.Unnamed")})" : volume!.Name;
        VolumeNameText.Foreground = BrushFor(syntheticName);
        VolumeNameText.Text = volumeName;
        SystemText.Text = CurrentSystem(document);
        ProtectionText.Text = LocExtension.Get("Explorer.Metadata.None");
        FileSystemText.Text = volume?.FileSystemId ?? ControlVisualConstants.EmptyValue;
        var capacity = volume?.Capacity ?? _mediaVolume?.Descriptor.Length;
        CapacityText.Text = capacity.HasValue ? StorageSizeFormatter.FormatBytes(capacity.Value) : ControlVisualConstants.EmptyValue;
        FreeText.Text = volume?.FreeSpaceKnown == true
            ? StorageSizeFormatter.FormatBytes(volume.FreeBytes)
            : ControlVisualConstants.EmptyValue;
        _rootEntries = volume?.Entries ?? [];
        EntryCountText.Text = CountEntries(_rootEntries).ToString();
        _rootFolder = new ExplorerFolderItem(volumeName, null, 0, _rootEntries, syntheticName) { IsExpanded = true };
        RefreshVisibleFolders(_rootFolder);
        FolderList.SelectedItem = _rootFolder;
        ShowContents(_rootEntries);

        var issues = _mediaVolume is null ? document.Diagnostics : ExplorerIssueBuilder.Build(document, _mediaVolume);
        WarningsButton.Visibility = issues.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        WarningsText.Text = $"{LocExtension.Get("Explorer.Warnings")} : {issues.Count}";
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
        var catalog = new DiskClassificationCatalog(_formats);
        var recognized = _detectedFormatIds.Select(id => catalog.ResolveFormat(id))
            .Where(format => format is not null).Cast<DiskFormat>()
            .DistinctBy(format => format.Id, StringComparer.OrdinalIgnoreCase)
            .Select(format => $"{format.Family} ({format.DisplayName})")
            .ToArray();
        var value = recognized.Length == 0 ? "\u2014" : string.Join("  \u00b7  ", recognized);
        return LocExtension.Get("Explorer.DetectedFormats", value);
    }

    private string CurrentSystem(ExploredDiskImage document)
    {
        if (Classification.SelectedMachine is { } selectedMachine)
        {
            return selectedMachine;
        }

        var format = new DiskClassificationCatalog(_formats).ResolveFormat(document.PrimaryFormatId);
        return format?.Family ?? ExplorerMetadataPresenter.Systems(document.Metadata);
    }

    private string CurrentSystem(ExploredMediaImage document)
    {
        if (Classification.SelectedMachine is { } selectedMachine) return selectedMachine;
        var format = new DiskClassificationCatalog(_formats).ResolveFormat(document.Document.FormatId);
        return format?.Family ?? document.Document.MediaKind.ToString();
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
