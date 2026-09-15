using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariBurgersPackedExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: 7216 }
        && data.Take(16).SequenceEqual(new byte[]
        {
            0xff, 0xff, 0x00, 0x40, 0x30, 0x5f, 0x00, 0x3e,
            0x00, 0x20, 0x9c, 0x35, 0x4c, 0x9c, 0x35, 0x00
        })
        && data.Distinct().Take(128).Count() == 128;
}
