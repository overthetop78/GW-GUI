using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariPrexorPackedExecutable(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 512 }
            || ReadUInt16(data, 0) != 0xffff
            || data.Count < 6)
            return false;
        var start = ReadUInt16(data, 2);
        var end = ReadUInt16(data, 4);
        return end >= start
            && end - start + 1 > data.Count
            && ContainsSequence(data, AtariPrexorRoutineSignature);
    }
}
