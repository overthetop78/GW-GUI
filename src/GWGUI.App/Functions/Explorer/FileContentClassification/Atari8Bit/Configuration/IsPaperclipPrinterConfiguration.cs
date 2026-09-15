using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsPaperclipPrinterConfiguration(FileSystemEntry entry)
    {
        if (entry.Content is not { Count: > 0 } data) return false;
        var extension = System.IO.Path.GetExtension(entry.Name);
        return string.Equals(extension, ".cnf", StringComparison.OrdinalIgnoreCase) && data.Count == 190
            || string.Equals(extension, ".cng", StringComparison.OrdinalIgnoreCase)
                && data.Count <= 64 && data.Contains((byte)0x9b) && data.Contains((byte)0xff);
    }
}
