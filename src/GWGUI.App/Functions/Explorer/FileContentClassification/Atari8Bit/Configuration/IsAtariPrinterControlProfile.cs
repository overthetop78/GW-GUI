using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariPrinterControlProfile(IReadOnlyList<byte>? data) =>
        data is { Count: 9 }
        && data[0] == 0x1b && data[2] == 0x9b
        && data[3] == 0x1b && data[5] == 0x9b
        && data[6] == 0x1b && data[8] == 0x9b;
}
