using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitExecutableWithEmbeddedPayload(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 12 } || ReadUInt16(data, 0) != 0xffff) return false;
        var offset = 2;
        var loadedRanges = new List<(int Start, int End)>();
        var validEntryVector = false;
        while (offset < data.Count)
        {
            while (offset + 1 < data.Count && ReadUInt16(data, offset) == 0xffff) offset += 2;
            if (offset + 4 > data.Count) return validEntryVector || HasEmbeddedAtariEntryVector(data, offset, loadedRanges);
            var start = ReadUInt16(data, offset);
            var end = ReadUInt16(data, offset + 2);
            if (end < start) return validEntryVector || HasEmbeddedAtariEntryVector(data, offset, loadedRanges);
            offset += 4;
            var length = end - start + 1;
            if (offset + length > data.Count)
            {
                var declaredRanges = loadedRanges.Append((start, end)).ToArray();
                return validEntryVector || HasEmbeddedAtariEntryVector(data, offset - 4, declaredRanges);
            }
            if (length == 2 && (start == 0x02e0 || start == 0x02e2))
            {
                var target = ReadUInt16(data, offset);
                validEntryVector = loadedRanges.Any(range => target >= range.Start && target <= range.End);
            }
            loadedRanges.Add((start, end));
            offset += length;
        }
        return false;
    }
}
