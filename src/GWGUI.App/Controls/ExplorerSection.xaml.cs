using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.Images;

namespace GWGUI.App.Controls;

public sealed record ExplorerFormatChoice(string? Id, string Name)
{
    public override string ToString() => Name;
}

public sealed class ExplorerEntryItem
{
    public ExplorerEntryItem(FileSystemEntry entry)
    {
        Entry = entry;
        Children = new(entry.Children.Select(child => new ExplorerEntryItem(child)));
    }

    public FileSystemEntry Entry { get; }
    public string Name => Entry.Name;
    public string Icon => Entry.Kind == FileSystemEntryKind.Directory ? "\uE8B7" : "\uE8A5";
    public string SizeText => Entry.Kind == FileSystemEntryKind.Directory ? string.Empty : FormatBytes(Entry.Size);
    public ObservableCollection<ExplorerEntryItem> Children { get; }

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

    public ExplorerSection()
    {
        InitializeComponent();
        RefreshFormats(null);
        OpenButton.Click += (_, e) => OpenRequested?.Invoke(this, e);
        ReadDiskButton.Click += (_, e) => ReadDiskRequested?.Invoke(this, e);
    }

    public event RoutedEventHandler? OpenRequested;
    public event RoutedEventHandler? ReadDiskRequested;
    public event EventHandler? FormatChanged;
    public Button OpenImageButton => OpenButton;
    public void SetReadDiskRunning(bool running) => ReadDiskButton.Content = LocExtension.Get(running ? "Common.Stop" : "Explorer.ReadDisk");
    public string? SelectedFormatId => (FormatCombo.SelectedItem as ExplorerFormatChoice)?.Id;

    public void RefreshFormats(string? selectedId)
    {
        _changingFormat = true;
        FormatCombo.ItemsSource = new[]
        {
            new ExplorerFormatChoice(null, LocExtension.Get("Explorer.Automatic")),
            new ExplorerFormatChoice("amigados", LocExtension.Get("Explorer.AmigaDos"))
        };
        FormatCombo.SelectedItem = FormatCombo.Items.Cast<ExplorerFormatChoice>().FirstOrDefault(item => item.Id == selectedId) ?? FormatCombo.Items[0];
        _changingFormat = false;
    }

    public void SetLoading(bool loading) => LoadingOverlay.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;

    public void Clear(string? path = null)
    {
        PathText.Text = path ?? string.Empty;
        VolumeNameText.Text = FileSystemText.Text = CapacityText.Text = FreeText.Text = EntryCountText.Text = "—";
        FileTree.ItemsSource = null;
        SelectedNameText.Text = LocExtension.Get("Explorer.SelectEntry");
        SelectedTypeText.Text = SelectedSizeText.Text = SelectedModifiedText.Text = SelectedCommentText.Text = "—";
        WarningTitle.Visibility = WarningsText.Visibility = Visibility.Collapsed;
    }

    public void Display(ExploredDiskImage document)
    {
        PathText.Text = document.SourcePath;
        VolumeNameText.Text = string.IsNullOrWhiteSpace(document.Volume.Name) ? LocExtension.Get("Explorer.Unnamed") : document.Volume.Name;
        FileSystemText.Text = document.Volume.FileSystem;
        CapacityText.Text = ExplorerEntryItem.FormatBytes(document.Volume.Capacity);
        FreeText.Text = ExplorerEntryItem.FormatBytes(document.Volume.FreeBytes);
        EntryCountText.Text = CountEntries(document.Volume.Entries).ToString();
        FileTree.ItemsSource = document.Volume.Entries.Select(entry => new ExplorerEntryItem(entry)).ToArray();
        SelectedNameText.Text = LocExtension.Get("Explorer.SelectEntry");
        SelectedTypeText.Text = SelectedSizeText.Text = SelectedModifiedText.Text = SelectedCommentText.Text = "—";
        var warnings = document.Volume.Warnings;
        WarningTitle.Visibility = WarningsText.Visibility = warnings.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        WarningsText.Text = string.Join(Environment.NewLine, warnings);
    }

    private static int CountEntries(IEnumerable<FileSystemEntry> entries) => entries.Sum(entry => 1 + CountEntries(entry.Children));

    private void FormatCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_changingFormat) FormatChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FileTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is not ExplorerEntryItem item) return;
        var entry = item.Entry;
        SelectedNameText.Text = entry.Name;
        SelectedTypeText.Text = LocExtension.Get("Explorer." + entry.Kind);
        SelectedSizeText.Text = ExplorerEntryItem.FormatBytes(entry.Size);
        SelectedModifiedText.Text = entry.Modified?.LocalDateTime.ToString("g") ?? "—";
        SelectedCommentText.Text = string.IsNullOrWhiteSpace(entry.Comment) ? "—" : entry.Comment;
    }

}
