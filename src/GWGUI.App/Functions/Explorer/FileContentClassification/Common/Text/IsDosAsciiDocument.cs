using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsDosAsciiDocument(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".asc", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: > 0 } data)
            return false;

        var hasCrLf = false;
        for (var index = 0; index < data.Count - 1; index++)
        {
            if (data[index] != 0x0d || data[index + 1] != 0x0a) continue;
            hasCrLf = true;
            break;
        }
        var textBytes = data.Count(value => value is 0x0a or 0x0d or 0x10 or 0x18 || value >= 0x20);
        return hasCrLf && textBytes >= data.Count * 0.98;
    }
}
