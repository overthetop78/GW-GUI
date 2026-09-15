using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsNewsroomPhoto(IReadOnlyList<byte>? data) =>
        data is { Count: >= 64 }
        && data[0] == 0xff && data[1] == 0xff && data[2] == 0x00 && data[3] == 0xa0
        && ContainsAscii(data, "NEWSROOM", 128);
}
