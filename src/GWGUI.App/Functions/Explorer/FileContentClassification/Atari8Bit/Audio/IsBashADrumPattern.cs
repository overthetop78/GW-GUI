using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsBashADrumPattern(IReadOnlyList<byte>? data) =>
        data is { Count: 2710 }
        && data[0] == 1 && data[1] == 0 && data[2] == 0
        && data[3] == 1 && data[4] == 1 && data[5] == 0;
}
