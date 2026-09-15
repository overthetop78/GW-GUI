using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariMegacolorEditorImage(FileSystemEntry entry) =>
        entry.Content is { Count: 7856 }
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".mga", StringComparison.OrdinalIgnoreCase);
}
