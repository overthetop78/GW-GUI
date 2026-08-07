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

public sealed record ExplorerFormatChoice(string? Id, string Name)
{
    public override string ToString() => Name;
}

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
    public ExplorerContentItem(FileSystemEntry entry)
    {
        Entry = entry;
        IconKind = ExplorerFileIconClassifier.IconFor(entry);
        TypeText = LocExtension.Get(ExplorerFileIconClassifier.TypeResourceKeyFor(IconKind));
    }

    public FileSystemEntry Entry { get; }
    public string Name => Entry.Name;
    public ExplorerIconKind IconKind { get; }
    public string TypeText { get; }
    public string SizeText => Entry.Kind == FileSystemEntryKind.Directory ? string.Empty : ExplorerFormatting.FormatBytes(Entry.Size);
    public string ModifiedText => Entry.Modified?.LocalDateTime.ToString("g") ?? "—";
}

public static class ExplorerFileIconClassifier
{
    private static readonly HashSet<string> Text = new(StringComparer.OrdinalIgnoreCase) { ".txt", ".nfo", ".info", ".doc", ".guide", ".readme", ".asm", ".s", ".c", ".h", ".bas", ".ini", ".cfg", ".xml", ".html" };
    private static readonly HashSet<string> Images = new(StringComparer.OrdinalIgnoreCase) { ".iff", ".ilbm", ".lbm", ".bmp", ".gif", ".jpg", ".jpeg", ".png", ".pcx", ".neo", ".pi1", ".pi2", ".pi3", ".pntg" };
    private static readonly HashSet<string> Audio = new(StringComparer.OrdinalIgnoreCase) { ".mod", ".med", ".xm", ".s3m", ".wav", ".8svx", ".voc", ".snd", ".sid" };
    private static readonly HashSet<string> Archives = new(StringComparer.OrdinalIgnoreCase) { ".lha", ".lzh", ".zip", ".arc", ".zoo", ".sit", ".dms", ".tar", ".gz" };
    private static readonly HashSet<string> Programs = new(StringComparer.OrdinalIgnoreCase) { ".exe", ".com", ".bat", ".cmd", ".prg", ".ttp", ".tos", ".app", ".library", ".device", ".handler" };
    private static readonly HashSet<string> DiskImages = new(StringComparer.OrdinalIgnoreCase) { ".adf", ".scp", ".hfe", ".ima", ".img", ".st", ".msa", ".atr", ".d64", ".d71", ".d81", ".ipf" };

    public static ExplorerIconKind IconFor(FileSystemEntry entry)
    {
        if (entry.Kind == FileSystemEntryKind.Directory) return ExplorerIconKind.Folder;
        if (entry.Kind == FileSystemEntryKind.Link) return ExplorerIconKind.Link;
        var extension = Path.GetExtension(entry.Name);
        if (Text.Contains(extension) || LooksLikeText(entry.Content)) return ExplorerIconKind.Text;
        if (Images.Contains(extension) || HasFormType(entry.Content, "ILBM")) return ExplorerIconKind.Image;
        if (Audio.Contains(extension) || HasFormType(entry.Content, "8SVX")) return ExplorerIconKind.Audio;
        if (Archives.Contains(extension)) return ExplorerIconKind.Archive;
        if (Programs.Contains(extension) || IsAmigaExecutable(entry.Content) || entry.Comment.StartsWith("PRG", StringComparison.OrdinalIgnoreCase)) return ExplorerIconKind.Program;
        if (DiskImages.Contains(extension)) return ExplorerIconKind.DiskImage;
        return ExplorerIconKind.File;
    }

    public static string TypeResourceKeyFor(ExplorerIconKind kind) => kind switch
    {
        ExplorerIconKind.Folder => "Explorer.Directory",
        ExplorerIconKind.Text => "Explorer.Type.Text",
        ExplorerIconKind.Image => "Explorer.Type.Image",
        ExplorerIconKind.Audio => "Explorer.Type.Audio",
        ExplorerIconKind.Archive => "Explorer.Type.Archive",
        ExplorerIconKind.Program => "Explorer.Type.Program",
        ExplorerIconKind.DiskImage => "Explorer.Type.DiskImage",
        ExplorerIconKind.Link => "Explorer.Link",
        _ => "Explorer.File"
    };

    private static bool IsAmigaExecutable(IReadOnlyList<byte>? data) => data is { Count: >= 4 } && data[0] == 0 && data[1] == 0 && data[2] == 3 && data[3] == 0xF3;

    private static bool HasFormType(IReadOnlyList<byte>? data, string type) => data is { Count: >= 12 } &&
        data[0] == (byte)'F' && data[1] == (byte)'O' && data[2] == (byte)'R' && data[3] == (byte)'M' &&
        data.Skip(8).Take(4).SequenceEqual(System.Text.Encoding.ASCII.GetBytes(type));

    private static bool LooksLikeText(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: > 0 }) return false;
        var sample = data.Take(Math.Min(data.Count, 512)).ToArray();
        var printable = sample.Count(value => value is 9 or 10 or 13 || value >= 32 && value < 127);
        return printable >= sample.Length * 0.9;
    }
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
    private bool _changingFormat;
    private ExplorerFolderItem? _rootFolder;
    private ExploredDiskImage? _document;
    private IReadOnlyList<FileSystemEntry> _rootEntries = [];
    private readonly ObservableCollection<ExplorerFolderItem> _visibleFolders = [];

    public ExplorerSection()
    {
        InitializeComponent();
        FolderList.ItemsSource = _visibleFolders;
        SetFormats([], null);
        OpenButton.Click += (_, e) => OpenRequested?.Invoke(this, e);
        ReadDiskButton.Click += (_, e) => ReadDiskRequested?.Invoke(this, e);
    }

    public event RoutedEventHandler? OpenRequested;
    public event RoutedEventHandler? ReadDiskRequested;
    public event EventHandler? FormatChanged;
    public Button OpenImageButton => OpenButton;
    public IReadOnlyList<ExplorerFormatChoice> FormatChoices => FormatCombo.Items.Cast<ExplorerFormatChoice>().ToArray();
    public void SetReadDiskRunning(bool running) => ReadDiskButton.Content = LocExtension.Get(running ? "Common.Stop" : "Explorer.ReadDisk");
    public string? SelectedFormatId => (FormatCombo.SelectedItem as ExplorerFormatChoice)?.Id;

    public void SetFormats(IEnumerable<DiskFormat> formats, string? selectedId)
    {
        _changingFormat = true;
        var choices = new List<ExplorerFormatChoice> { new(null, LocExtension.Get("Explorer.Automatic")) };
        choices.AddRange(formats.Select(format => new ExplorerFormatChoice(format.Id, format.DisplayName)));
        FormatCombo.ItemsSource = choices;
        FormatCombo.SelectedItem = choices.FirstOrDefault(item => item.Id == selectedId) ?? choices[0];
        _changingFormat = false;
    }

    public void SetLoading(bool loading) => LoadingOverlay.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;

    public void Clear(string? path = null)
    {
        PathText.Text = path ?? string.Empty;
        VolumeNameText.Text = FileSystemText.Text = CapacityText.Text = FreeText.Text = EntryCountText.Text = "—";
        _rootFolder = null;
        _document = null;
        _rootEntries = [];
        _visibleFolders.Clear();
        ContentsList.ItemsSource = null;
        WarningsBadge.Visibility = Visibility.Collapsed;
        DetailsPanel.Clear();
    }

    public void Display(ExploredDiskImage document)
    {
        _document = document;
        PathText.Text = document.SourcePath;
        var volumeName = !document.FileSystemRecognized
            ? LocExtension.Get("Explorer.Unknown")
            : string.IsNullOrWhiteSpace(document.Volume.Name) ? LocExtension.Get("Explorer.Unnamed") : document.Volume.Name;
        VolumeNameText.Text = volumeName;
        FileSystemText.Text = document.FileSystemRecognized ? document.Volume.FileSystem : LocExtension.Get("Explorer.Unknown");
        CapacityText.Text = ExplorerFormatting.FormatBytes(document.Volume.Capacity);
        FreeText.Text = document.FileSystemRecognized ? ExplorerFormatting.FormatBytes(document.Volume.FreeBytes) : "\u2014";
        EntryCountText.Text = CountEntries(document.Volume.Entries).ToString();
        _rootEntries = document.Volume.Entries;
        _rootFolder = new ExplorerFolderItem(volumeName, null, 0, _rootEntries) { IsExpanded = true };
        RefreshVisibleFolders(_rootFolder);
        FolderList.SelectedItem = _rootFolder;
        ShowContents(_rootEntries);
        DetailsPanel.ShowDisk(document);
        var warningCount = document.Volume.Warnings.Count;
        WarningsBadge.Visibility = warningCount == 0 ? Visibility.Collapsed : Visibility.Visible;
        WarningsText.Text = $"{LocExtension.Get("Explorer.Warnings")} : {warningCount}";
    }

    public static int CountEntries(IEnumerable<FileSystemEntry> entries) => entries.Sum(entry => 1 + CountEntries(entry.Children));

    private void RefreshVisibleFolders(ExplorerFolderItem? selected = null)
    {
        if (_rootFolder is null) return;
        _visibleFolders.Clear();
        AddVisible(_rootFolder);
        if (selected is not null) FolderList.SelectedItem = selected;
    }

    private void AddVisible(ExplorerFolderItem item)
    {
        _visibleFolders.Add(item);
        if (item.IsExpanded) foreach (var child in item.Children) AddVisible(child);
    }

    private void ShowContents(IEnumerable<FileSystemEntry> entries)
    {
        ContentsList.ItemsSource = entries
            .OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory)
            .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(entry => new ExplorerContentItem(entry)).ToArray();
        ContentsList.SelectedItem = null;
        if (_document is not null) DetailsPanel.ShowDisk(_document);
    }

    private void ContentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_document is null) return;
        if (ContentsList.SelectedItem is ExplorerContentItem item) DetailsPanel.ShowItem(_document, item);
        else DetailsPanel.ShowDisk(_document);
    }

    private void FormatCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_changingFormat) FormatChanged?.Invoke(this, EventArgs.Empty);
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
        var folder = FindFolder(_rootFolder, selected.Entry);
        if (folder is null) return;
        ExpandAncestors(_rootFolder, folder);
        RefreshVisibleFolders(folder);
        ShowContents(folder.Entry!.Children);
    }

    private static ExplorerFolderItem? FindFolder(ExplorerFolderItem current, FileSystemEntry entry)
    {
        if (ReferenceEquals(current.Entry, entry)) return current;
        foreach (var child in current.Children)
        {
            var found = FindFolder(child, entry);
            if (found is not null) return found;
        }
        return null;
    }

    private static bool ExpandAncestors(ExplorerFolderItem current, ExplorerFolderItem target)
    {
        if (ReferenceEquals(current, target)) return true;
        foreach (var child in current.Children)
        {
            if (!ExpandAncestors(child, target)) continue;
            current.IsExpanded = true;
            return true;
        }
        return false;
    }
}
