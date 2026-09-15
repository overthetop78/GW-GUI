using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariMarcoPixelEditorImage(FileSystemEntry entry) =>
        entry.Content is { Count: > 0 }
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".cpi", StringComparison.OrdinalIgnoreCase);
}
