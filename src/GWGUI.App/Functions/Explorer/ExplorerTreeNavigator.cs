using GWGUI.App.ViewModels.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static class ExplorerTreeNavigator
{
    public static IEnumerable<ExplorerFolderItem> Flatten(ExplorerFolderItem root)
    {
        yield return root;
        if (!root.IsExpanded) yield break;
        foreach (var child in root.Children)
            foreach (var visible in Flatten(child))
                yield return visible;
    }

    public static ExplorerFolderItem? Find(ExplorerFolderItem current, FileSystemEntry entry)
    {
        if (ReferenceEquals(current.Entry, entry)) return current;
        foreach (var child in current.Children)
        {
            var found = Find(child, entry);
            if (found is not null) return found;
        }
        return null;
    }

    public static bool ExpandPathTo(ExplorerFolderItem current, ExplorerFolderItem target)
    {
        if (ReferenceEquals(current, target)) return true;
        foreach (var child in current.Children)
        {
            if (!ExpandPathTo(child, target)) continue;
            current.IsExpanded = true;
            return true;
        }
        return false;
    }
}
