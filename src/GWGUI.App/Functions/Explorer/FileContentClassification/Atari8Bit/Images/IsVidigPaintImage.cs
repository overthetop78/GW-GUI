using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsVidigPaintImage(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".rap", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 7681 } data
        && data.Take(7680).Distinct().Skip(1).Any();
}
