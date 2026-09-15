using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariUnicumLevel(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".uni", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 3044 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x00, 0x00, 0x32, 0x04, 0x12, 0x72, 0xd2, 0x72,
            0x52, 0x32, 0x92, 0x92, 0xa2, 0x32, 0x04, 0x62
        })
        && data.Distinct().Take(64).Count() == 64;
}
