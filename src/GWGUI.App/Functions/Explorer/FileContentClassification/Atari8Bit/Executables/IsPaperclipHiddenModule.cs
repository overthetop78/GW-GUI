using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsPaperclipHiddenModule(IReadOnlyList<byte>? data) =>
        data is { Count: 241 }
        && data[0] == 0x29 && data[1] == 0x13 && data[2] == 0x9d
        && data[3] == 0x31 && data[4] == 0x13 && data[5] == 0xf0;
}
