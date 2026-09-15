using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsTrickBinaryContainer(FileSystemEntry entry) =>
        string.IsNullOrEmpty(System.IO.Path.GetExtension(entry.Name))
        && entry.Content is { Count: >= 3000 } data
        && data[0] == 0xfb && data[1] == 0xc2
        && data[data.Count - 4] == 0xff && data[data.Count - 3] == 0xff;
}
