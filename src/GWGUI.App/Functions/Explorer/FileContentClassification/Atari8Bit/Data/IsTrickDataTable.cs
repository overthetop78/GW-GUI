using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsTrickDataTable(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".dan", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 336 } data
        && data.Take(12).SequenceEqual(new byte[] { 0x00, 0x00, 0x67, 0x00, 0x01, 0x61, 0x00, 0x02, 0x61, 0x00, 0x03, 0x69 });
}
