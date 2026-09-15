using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAwardWareCatalog(IReadOnlyList<byte>? data) =>
        data is { Count: 1605 }
        && data[0] == 0x40 && data[1] == 0x01
        && data[2] == 0xc0 && data[3] == 0x00;
}
