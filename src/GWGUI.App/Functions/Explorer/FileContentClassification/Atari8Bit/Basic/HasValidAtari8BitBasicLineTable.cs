using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool HasValidAtari8BitBasicLineTable(IReadOnlyList<byte> data, int offset)
    {
        var lines = 0;
        var previousLine = -1;
        while (offset < data.Count)
        {
            if (offset + 3 > data.Count) return lines >= 3;
            var line = ReadUInt16(data, offset);
            var length = data[offset + 2];
            if (line < previousLine || length < 4 || offset + length > data.Count) return lines >= 3;
            previousLine = line;
            offset += length;
            lines++;
        }
        return lines > 0;
    }
}
