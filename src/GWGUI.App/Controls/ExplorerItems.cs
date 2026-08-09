using System.Windows;
using GWGUI.App.Localization;
using GWGUI.Scp.FileSystems;

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
    public string ModifiedText => Entry.Modified?.LocalDateTime.ToString("g") ?? "\u2014";
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
