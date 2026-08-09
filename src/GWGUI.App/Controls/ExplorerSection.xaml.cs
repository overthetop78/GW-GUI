using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GWGUI.App.Localization;
using GWGUI.Domain.Formats;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.Images;

namespace GWGUI.App.Controls;

public sealed record ExplorerFormatChoice(string? Id, string Name);

public sealed class ExplorerFolderItem
{
    public ExplorerFolderItem(string name, FileSystemEntry? entry, int depth, IEnumerable<FileSystemEntry> children)
    {
        Name = name;
        Entry = entry;
        Depth = depth;
        Children = children.Where(child => child.Kind == FileSystemEntryKind.Directory)
            .Select(child => new ExplorerFolderItem(child.Name, child, depth + 1, child.Children)).ToArray();
    }

    public string Name { get; }
    public FileSystemEntry? Entry { get; }
    public int Depth { get; }
    public IReadOnlyList<ExplorerFolderItem> Children { get; }
    public bool IsExpanded { get; set; }
    public string ToggleText => Children.Count == 0 ? string.Empty : IsExpanded ? "-" : "+";
    public Thickness Indent => new(Depth * 17, 0, 0, 0);
}

public sealed class ExplorerContentItem
{
    public ExplorerContentItem(FileSystemEntry entry, ExplorerFileSystemFamily family = ExplorerFileSystemFamily.Unknown)
    {
        Entry = entry;
        IconKind = ExplorerFileIconClassifier.IconFor(entry, family);
        TypeText = LocExtension.Get(ExplorerFileIconClassifier.TypeResourceKeyFor(IconKind));
    }

    public FileSystemEntry Entry { get; }
    public string Name => Entry.Name;
    public ExplorerIconKind IconKind { get; }
    public string TypeText { get; }
    public string SizeText => Entry.Kind == FileSystemEntryKind.Directory ? string.Empty : ExplorerFormatting.FormatBytes(Entry.Size);
    public string ModifiedText => Entry.Modified?.LocalDateTime.ToString("g") ?? "—";
}

public static class ExplorerFormatting
{
    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024d:0.#} KiB";
        return $"{bytes / 1024d / 1024d:0.##} MiB";
    }
}

public partial class ExplorerSection : UserControl
{
    private ExplorerFolderItem? _rootFolder;
    private ExploredDiskImage? _document;
    private IReadOnlyList<FileSystemEntry> _rootEntries = [];
    private IReadOnlyList<DiskFormat> _formats = [];
    private readonly ObservableCollection<ExplorerFolderItem> _visibleFolders = [];

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
    public string? FormatIdForLoading => AutomaticDetection.IsChecked == true ? null : SelectedFormatId;

    private void AutomaticDetection_Changed(object sender, RoutedEventArgs e)
    {
        var enabled = AutomaticDetection.IsChecked == true;
        Classification.SetAutomaticDetection(enabled);
        if (enabled && _document is not null)
            Classification.ApplyDetection(_document.Image.FormatId,
                _document.Metadata.ProtectionName is null ? null : "apple2.rwts18");
    }

    public void SetFormats(IEnumerable<DiskFormat> formats, string? selectedId)
    {
        var hadSelection = Classification.SelectedFormatId is not null;
        _formats = formats.ToArray();
        Classification.SetCatalog(_formats);
        if (!hadSelection && selectedId is not null) Classification.ApplyDetection(selectedId, null);
    }

    public void SetLoading(bool loading) => LoadingOverlay.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;

    public void Clear(string? path = null)
    {
        PathText.Text = path ?? string.Empty;
        VolumeNameText.Text = FileSystemText.Text = CapacityText.Text = FreeText.Text = EntryCountText.Text = "—";
        SystemText.Text = ProtectionText.Text = "\u2014";
        _rootFolder = null;
        _document = null;
        _rootEntries = [];
        _visibleFolders.Clear();
        ContentsList.ItemsSource = null;
        WarningsButton.Visibility = Visibility.Collapsed;
        DetailsPanel.Clear();
    }

    public void Display(ExploredDiskImage document)
    {
        _document = document;
        PathText.Text = document.SourcePath;
        Classification.SetAutomaticDetection(AutomaticDetection.IsChecked == true);
        Classification.ApplyDetection(document.Image.FormatId,
            document.Metadata.ProtectionName is null ? null : "apple2.rwts18");
        var volumeName = !document.FileSystemRecognized
            ? LocExtension.Get("Explorer.Unknown")
            : string.IsNullOrWhiteSpace(document.Volume.Name) ? LocExtension.Get("Explorer.Unnamed") : document.Volume.Name;
        VolumeNameText.Text = volumeName;
        SystemText.Text = document.Metadata.SystemName;
        ProtectionText.Text = document.Metadata.ProtectionName ?? "\u2014";
        FileSystemText.Text = ExplorerDetailsPresenter.FileSystemText(document);
        CapacityText.Text = ExplorerFormatting.FormatBytes(document.Volume.Capacity);
        FreeText.Text = document.FileSystemRecognized ? ExplorerFormatting.FormatBytes(document.Volume.FreeBytes) : "\u2014";
        EntryCountText.Text = CountEntries(document.Volume.Entries).ToString();
        _rootEntries = document.Volume.Entries;
        _rootFolder = new ExplorerFolderItem(volumeName, null, 0, _rootEntries) { IsExpanded = true };
        RefreshVisibleFolders(_rootFolder);
        FolderList.SelectedItem = _rootFolder;
        ShowContents(_rootEntries);
        DetailsPanel.ShowDisk(document);
        var warningCount = BuildIssues(document).Count;
        WarningsButton.Visibility = warningCount == 0 ? Visibility.Collapsed : Visibility.Visible;
        WarningsText.Text = $"{LocExtension.Get("Explorer.Warnings")} : {warningCount}";
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
        var family = _document is null ? ExplorerFileSystemFamily.Unknown : ExplorerFileIconClassifier.FamilyFor(_document);
        ContentsList.ItemsSource = entries
            .OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory)
            .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(entry => new ExplorerContentItem(entry, family)).ToArray();
        ContentsList.SelectedItem = null;
        if (_document is not null) DetailsPanel.ShowDisk(_document);
    }

    private void ContentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_document is null) return;
        if (ContentsList.SelectedItem is ExplorerContentItem item) DetailsPanel.ShowItem(_document, item);
        else DetailsPanel.ShowDisk(_document);
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
        if (_document is null) return;
        new ExplorerIssuesWindow(BuildIssues(_document)) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    public static IReadOnlyList<string> BuildIssues(ExploredDiskImage document) => ExplorerIssueBuilder.Build(document);
}
