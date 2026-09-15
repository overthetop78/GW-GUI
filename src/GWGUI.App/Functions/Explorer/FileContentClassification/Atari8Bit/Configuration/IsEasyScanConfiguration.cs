using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsEasyScanConfiguration(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: 120 } || data.Take(28).Any(value => value != 0) || data[28] != 2 || data[29] != 1)
            return false;
        for (var index = 0; index < 16; index++)
        {
            if (data[53 + index] != (byte)(((index + 1) & 15) << 4)) return false;
        }
        return true;
    }
}
