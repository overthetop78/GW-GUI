using System.Windows;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.ViewModels.Explorer;

public sealed class ExplorerFolderItem
{
    public ExplorerFolderItem(string name, FileSystemEntry? entry, int depth, IEnumerable<FileSystemEntry> children, bool isSyntheticName = false)
    {
        Name = name;
        Entry = entry;
        Depth = depth;
        IsSyntheticName = isSyntheticName;
        Children = children.Where(child => child.Kind == FileSystemEntryKind.Directory)
            .Select(child => new ExplorerFolderItem(child.Name, child, depth + 1, child.Children)).ToArray();
    }

    public string Name { get; }
    public FileSystemEntry? Entry { get; }
    public int Depth { get; }
    public bool IsSyntheticName { get; }
    public IReadOnlyList<ExplorerFolderItem> Children { get; }
    public bool IsExpanded { get; set; }
    public string ToggleText => Children.Count == 0 ? string.Empty : IsExpanded ? "-" : "+";
    public Thickness Indent => new(Depth * 17, 0, 0, 0);
}
