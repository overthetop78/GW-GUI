using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitDrumPattern(IReadOnlyList<byte>? data) =>
        data is { Count: 282 }
        && data[0] == 0x42 && data[1] == 0x01
        && data[2] == 0x23 && data[3] == 0x45;
}
