using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsTrickFont(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".pfd", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 965 } data
        && data.Take(6).SequenceEqual(new byte[] { 0xb3, 0xa6, 0xaa, 0x34, 0x00, 0x00 });
}
