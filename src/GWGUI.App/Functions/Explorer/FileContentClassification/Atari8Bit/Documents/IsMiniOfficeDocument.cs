using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsMiniOfficeDocument(IReadOnlyList<byte>? data) =>
        data is { Count: >= 64 }
        && data[0] == 0xcd && data[1] == 0xc4 && data[2] == 0x01 && data[3] == 0x0d
        && data.Contains((byte)0x9b);
}
