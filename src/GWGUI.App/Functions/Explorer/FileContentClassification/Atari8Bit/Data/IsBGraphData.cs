using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsBGraphData(IReadOnlyList<byte>? data) =>
        data is { Count: > 1024 }
        && data[0] == (byte)'0' && data[1] == 0x9b
        && data[2] == (byte)'0' && data[3] == 0x9b
        && data[4] == (byte)'1' && data[5] == (byte)'4' && data[6] == 0x9b;
}
