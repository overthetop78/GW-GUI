using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsBearEssentialsPicture(IReadOnlyList<byte>? data) =>
        data is { Count: 8320 }
        && data[0] == 0x48 && data[1] == 0xad && data[2] == 0x6e && data[3] == 0x06
        && data[4] == 0x8d && data[5] == 0x0a && data[6] == 0xd4;
}
