using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAwardWareGraphic(IReadOnlyList<byte>? data) =>
        data is { Count: > 900 }
        && ((data[0] == 0x90 && data[1] == 0x00 && data[2] == 0x70 && data[3] == 0x00)
            || (data.Count == 7722 && data[0] == 0x14 && data[1] == 0x00
                && data[4] == 0x80 && data[5] == 0x01));
}
