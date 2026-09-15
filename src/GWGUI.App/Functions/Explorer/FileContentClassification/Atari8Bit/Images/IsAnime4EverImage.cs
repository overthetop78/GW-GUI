using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAnime4EverImage(FileSystemEntry entry) =>
        entry.Content is { Count: >= 512 } data
        && data[2] == 0x00
        && data[3] == 0x90
        && data[4] == 0x4f
        && data[^3] == 0xdb
        && data[^2] == 0x01
        && data[^1] == 0x00;
}
