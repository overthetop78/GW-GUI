using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsWritersToolExtensionModule(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".ext", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 256 } data
        && data[0] == 0x4c
        && (ContainsAscii(data, "The Writer's Tool", data.Count)
            || ContainsAscii(data, "THE WRITER'S TOOL", data.Count));
}
