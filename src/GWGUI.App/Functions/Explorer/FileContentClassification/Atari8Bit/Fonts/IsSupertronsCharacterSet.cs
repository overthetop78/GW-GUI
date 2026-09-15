using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsSupertronsCharacterSet(FileSystemEntry entry) =>
        string.Equals(entry.Name, "ZSP", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 1024 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00,
            0x18, 0x18, 0x18, 0x18, 0x18, 0x00, 0x18, 0x00
        })
        && data.Count(value => value == 0) >= 128
        && data.Distinct().Take(32).Count() == 32;
}
