using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariMadDesignerImage(FileSystemEntry entry) =>
        entry.Content is { Count: 16384 }
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".mbg", StringComparison.OrdinalIgnoreCase);
}
