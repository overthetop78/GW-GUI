using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariRawScreenImage(IReadOnlyList<byte>? data) =>
        data is { Count: 3560 or 7625 or 7680 or 7900 or 8000 }
        || data is { Count: 10003 } && data[0] == 0x00 && data[1] == 0x40
        || data is { Count: 10018 } && data[0] == 0x00 && data[1] == 0x20;
}
