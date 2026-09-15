using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsLasertellerCore(IReadOnlyList<byte>? data) =>
        data is { Count: >= 512 }
        && data[0] == 0x4c && data[1] == 0xd4 && data[2] == 0x46 && data[3] == 0x48
        && ContainsAscii(data, "D:LASXML.DAT", 256)
        && ContainsAscii(data, "D:COREML.DAT", 256);
}
