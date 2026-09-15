using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsMidiPatternEditorComposition(FileSystemEntry entry) =>
        entry.Content is { Count: >= 1024 } data
        && data.Count % 256 == 0
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".mpe", StringComparison.OrdinalIgnoreCase)
        && data.Take(256).Any(value => value is 0xfe or 0xff);
}
