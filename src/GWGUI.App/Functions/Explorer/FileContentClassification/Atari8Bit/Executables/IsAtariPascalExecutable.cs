using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariPascalExecutable(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 10 }
            || ReadUInt16(data, 0) != 0xffff
            || ReadUInt16(data, 2) != 0x2000)
            return false;
        var endAddress = ReadUInt16(data, 4);
        if (endAddress < 0x2000) return false;
        var entryOffset = 6 + endAddress - 0x2000 + 1;
        return entryOffset + 4 == data.Count
            && ReadUInt16(data, entryOffset) == 0x2000
            && ReadUInt16(data, entryOffset + 2) == 0x2000;
    }
}
