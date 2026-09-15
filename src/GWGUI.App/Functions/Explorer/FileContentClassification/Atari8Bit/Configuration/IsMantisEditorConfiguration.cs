using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsMantisEditorConfiguration(FileSystemEntry entry) =>
        entry.Content is { Count: 42 }
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".ecf", StringComparison.OrdinalIgnoreCase);
}
