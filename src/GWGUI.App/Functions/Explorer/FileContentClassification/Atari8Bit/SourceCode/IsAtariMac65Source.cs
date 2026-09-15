using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariMac65Source(IReadOnlyList<byte>? data) =>
        data is { Count: >= 32 }
        && data[0] == 0xfe && data[1] == 0xfe
        && ContainsAscii(data, ";", 512);
}
