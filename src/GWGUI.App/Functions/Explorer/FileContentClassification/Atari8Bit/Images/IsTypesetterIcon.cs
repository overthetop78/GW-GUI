using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsTypesetterIcon(FileSystemEntry entry) =>
        entry.Content is { Count: 1791 }
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".ts", StringComparison.OrdinalIgnoreCase);
}
