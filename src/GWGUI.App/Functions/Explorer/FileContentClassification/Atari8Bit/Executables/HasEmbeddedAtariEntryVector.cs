using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool HasEmbeddedAtariEntryVector(
        IReadOnlyList<byte> data,
        int searchOffset,
        IReadOnlyList<(int Start, int End)> loadedRanges)
    {
        for (var offset = Math.Max(0, searchOffset); offset + 5 < data.Count; offset++)
        {
            var start = ReadUInt16(data, offset);
            if (start is not (0x02e0 or 0x02e2) || ReadUInt16(data, offset + 2) != start + 1) continue;
            var target = ReadUInt16(data, offset + 4);
            if (loadedRanges.Any(range => target >= range.Start && target <= range.End)) return true;
        }
        return false;
    }
}
