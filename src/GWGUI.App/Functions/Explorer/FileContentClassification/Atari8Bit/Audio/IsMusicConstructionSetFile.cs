using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsMusicConstructionSetFile(IReadOnlyList<byte>? data) =>
        data is { Count: >= 17 }
        && (data[0] == 0x1f && data[1] == 0x0a && data[2] == 0x10 && data[3] == 0x00
            || data.Skip(4).Take(13).SequenceEqual(new byte[]
            {
                0x01, 0x09, 0x01, 0x0d, 0x0d, 0x00, 0x00, 0xf1, 0xea, 0xdc, 0xff, 0xf1, 0xea
            }));
}
