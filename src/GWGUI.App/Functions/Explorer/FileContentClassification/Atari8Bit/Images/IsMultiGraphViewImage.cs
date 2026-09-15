using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsMultiGraphViewImage(FileSystemEntry entry)
    {
        if (entry.Content is not { Count: > 0 } data) return false;
        var extension = System.IO.Path.GetExtension(entry.Name);
        if ((string.Equals(extension, ".256", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".sfd", StringComparison.OrdinalIgnoreCase))
            && HasAsciiPrefix(data, "S101"))
            return true;
        return string.Equals(extension, ".apa", StringComparison.OrdinalIgnoreCase) && data.Count == 7720
            || string.Equals(extension, ".inp", StringComparison.OrdinalIgnoreCase) && data.Count == 16004;
    }
}
