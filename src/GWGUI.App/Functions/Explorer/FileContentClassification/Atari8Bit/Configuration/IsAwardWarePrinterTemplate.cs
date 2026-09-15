using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAwardWarePrinterTemplate(IReadOnlyList<byte>? data) =>
        data is { Count: 323 }
        && data[0] == 0x02 && data[1] == 0x89
        && data[2] == 0x54 && data[3] == 0x00;
}
