using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitDiskBootProgram(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 128 }
            || data[1] == 0
            || data.Count != data[1] * 128)
            return false;

        var loadAddress = ReadUInt16(data, 2);
        var continuationAddress = ReadUInt16(data, 4);
        return loadAddress > 0
            && loadAddress + data.Count <= 0x10000
            && continuationAddress > 0;
    }
}
