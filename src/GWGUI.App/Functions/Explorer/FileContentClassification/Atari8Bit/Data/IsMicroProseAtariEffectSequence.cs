using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsMicroProseAtariEffectSequence(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".eff", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 480 } data
        && data.Take(12).SequenceEqual(new byte[]
        {
            0x81, 0x02, 0x00, 0x05, 0x0a, 0x06, 0x01, 0xff,
            0x01, 0x01, 0x01, 0x01
        })
        && data.Count(value => value == 0x80) >= 200
        && data.Count(value => value == 0x81) >= 8
        && data.Count(value => value == 0x01) >= 80;
}
