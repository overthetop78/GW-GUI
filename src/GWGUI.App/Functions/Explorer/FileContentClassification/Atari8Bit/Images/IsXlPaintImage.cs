using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsXlPaintImage(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".xlp", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: > 256 and <= 32768 } data
        && (data[0] & 0x0f) == 0x04
        && (data[1] & 0x0f) == 0x08
        && (data[2] & 0x0f) == 0x0c
        && data[3] == 0x00
        && data.Skip(4).Distinct().Take(32).Count() == 32;
}
