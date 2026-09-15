using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsTipImage(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".tip", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 9 } data
        && data[0] == (byte)'T' && data[1] == (byte)'I' && data[2] == (byte)'P'
        && data[3] == 0x01 && data[4] == 0x00 && data[5] == 0xa0;
}
