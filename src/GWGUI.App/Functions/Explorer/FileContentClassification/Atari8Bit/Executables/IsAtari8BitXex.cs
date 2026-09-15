using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitXex(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 7 } || ReadUInt16(data, 0) != 0xffff) return false;
        var offset = 2;
        var segments = 0;
        while (offset < data.Count)
        {
            while (offset + 1 < data.Count && ReadUInt16(data, offset) == 0xffff) offset += 2;
            if (segments > 0 && data.Skip(offset).All(value => value is 0 or 0x1a or 0x9b)) return true;
            if (offset + 4 > data.Count) return false;
            var start = ReadUInt16(data, offset);
            var end = ReadUInt16(data, offset + 2);
            if (end < start) return false;
            offset += 4;
            var length = end - start + 1;
            if (offset + length > data.Count) return false;
            offset += length;
            segments++;
        }
        return segments > 0;
    }
}
