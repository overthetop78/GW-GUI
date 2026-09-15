using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariGraphicsDefinition(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".gdf", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 16 and <= 2048 } data
        && data.Count % 8 == 0
        && data.Distinct().Take(8).Count() == 8;
}
