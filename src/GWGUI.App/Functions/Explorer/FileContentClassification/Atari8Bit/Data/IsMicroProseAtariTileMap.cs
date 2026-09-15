using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsMicroProseAtariTileMap(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".map", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: >= 66 and <= 8192 } data)
            return false;

        var width = data[0];
        var height = data[1];
        if (width < 8 || height < 8) return false;
        var headerLength = data.Count - width * height;
        return headerLength is 2 or 42
            && data.Skip(headerLength).Distinct().Take(16).Count() == 16;
    }
}
