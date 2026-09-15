using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsBGraphMachineRoutine(IReadOnlyList<byte>? data) =>
        data is { Count: >= 100 }
        && ((data[0] == 0x68 && data[1] == 0x68 && data[2] == 0x68 && data[3] == 0x85 && data[4] == 0xe5)
            || (data[0] == 0xd8 && data[1] == 0x68 && data[2] == 0x68 && data[3] == 0x85 && data[4] == 0xcc)
            || (data[0] == 0xa4 && data[1] == 0x57 && data[2] == 0xa9 && data[3] == 0x28 && data[4] == 0xc0));
}
