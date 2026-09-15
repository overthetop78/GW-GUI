using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitDosBootImageFile(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 125 }
            || data[0] != 0
            || data[1] == 0
            || data.Count <= (data[1] - 1) * 125
            || data.Count > data[1] * 125)
            return false;

        var loadAddress = ReadUInt16(data, 2);
        var continuationAddress = ReadUInt16(data, 4);
        return loadAddress >= 0x0400
            && loadAddress + data[1] * 128 <= 0x10000
            && continuationAddress >= loadAddress
            && continuationAddress < loadAddress + data[1] * 128;
    }
}
