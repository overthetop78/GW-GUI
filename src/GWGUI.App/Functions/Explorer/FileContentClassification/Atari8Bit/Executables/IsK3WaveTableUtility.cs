using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsK3WaveTableUtility(IReadOnlyList<byte>? data) =>
        data is { Count: >= 5 }
        && (data[0] == 0x20 && data[1] == 0xc3 && data[2] == 0x55 && data[3] == 0x4c && data[4] == 0xbb
            || data[0] == 0x20 && data[1] == 0x03 && data[2] == 0x50 && data[3] == 0xa2 && data[4] == 0x70
            || data[0] == 0x4c && data[1] == 0x42 && data[2] == 0x50 && data[3] == 0xa2 && data[4] == 0x70);
}
