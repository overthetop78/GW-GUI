using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsWritersToolPrinterProfile(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".ppp", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 680 } data
        && data.Take(6).SequenceEqual(new byte[] { 0xff, 0xff, 0x00, 0x6f, 0xb2, 0x71 })
        && ContainsAscii(data, "AT825", 32)
        && data.Count(value => value == 0x1b) >= 12;
}
