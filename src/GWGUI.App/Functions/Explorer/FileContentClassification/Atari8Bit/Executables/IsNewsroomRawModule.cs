using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsNewsroomRawModule(IReadOnlyList<byte>? data) =>
        data is { Count: >= 5 }
        && data[0] == 0x20 && data[1] == 0xe0 && data[2] == 0x07
        && data[3] == 0xd8 && data[4] == 0x4c;
}
