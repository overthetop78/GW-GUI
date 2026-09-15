using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariGtfImage(FileSystemEntry entry) =>
        entry.Content is { Count: >= 11 } data
        && data[0] == (byte)'G'
        && data[1] == (byte)'T'
        && data[2] == (byte)'F'
        && data[3] == 0
        && data[4] > 0
        && data[5] > 0
        && data.Count == 10 + (data[4] * data[5] / 2);
}
