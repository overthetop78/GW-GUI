using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool LooksLikeAtasciiText(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: > 0 }) return false;
        var sampleLength = Math.Min(data.Count, 512);
        var printable = 0;
        var hasAtasciiEndOfLine = false;
        for (var index = 0; index < sampleLength; index++)
        {
            var value = data[index];
            if (value == 0x9b)
            {
                printable++;
                hasAtasciiEndOfLine = true;
            }
            else if ((value & 0x7f) is >= 32 and < 127 || value is 9 or 10 or 13)
            {
                printable++;
            }
        }
        return hasAtasciiEndOfLine && printable >= sampleLength * 0.9;
    }
}
