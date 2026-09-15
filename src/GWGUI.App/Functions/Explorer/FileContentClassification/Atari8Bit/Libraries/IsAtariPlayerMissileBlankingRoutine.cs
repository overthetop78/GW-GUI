using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariPlayerMissileBlankingRoutine(FileSystemEntry entry) =>
        string.Equals(entry.Name, "PMBLANK.FIL", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 256 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0xa2, 0x03, 0xbd, 0xf4, 0x06, 0xf0, 0x59, 0x38,
            0xdd, 0xf0, 0x06, 0xf0, 0x53, 0x8d, 0xfe, 0x06
        })
        && data.Skip(155).Take(16).SequenceEqual(new byte[]
        {
            0x4c, 0x62, 0xe4, 0x00, 0x00, 0x68, 0xa9, 0x07,
            0xa2, 0x06, 0xa0, 0x00, 0x20, 0x5c, 0xe4, 0x60
        })
        && data.Skip(171).All(value => value == 0);
}
