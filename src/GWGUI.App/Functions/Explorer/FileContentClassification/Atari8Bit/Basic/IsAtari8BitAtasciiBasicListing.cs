using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitAtasciiBasicListing(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 32 } || data[^1] != 0x9b) return false;
        var offset = 0;
        var lines = 0;
        var previousLine = -1;
        while (offset < data.Count)
        {
            var line = 0;
            var digitCount = 0;
            while (offset < data.Count && data[offset] is >= (byte)'0' and <= (byte)'9' && digitCount < 5)
            {
                line = line * 10 + data[offset] - (byte)'0';
                offset++;
                digitCount++;
            }
            if (digitCount == 0 || offset >= data.Count || data[offset] != (byte)' ' || line < previousLine)
                return false;
            previousLine = line;
            var lineEnd = offset + 1;
            while (lineEnd < data.Count && data[lineEnd] != 0x9b) lineEnd++;
            if (lineEnd >= data.Count) return false;
            lines++;
            offset = lineEnd + 1;
        }
        return lines >= 3
            && (ContainsAscii(data, " POKE ", data.Count)
                || ContainsAscii(data, "GRAPHICS ", data.Count))
            && ContainsAscii(data, "USR(", data.Count);
    }
}
