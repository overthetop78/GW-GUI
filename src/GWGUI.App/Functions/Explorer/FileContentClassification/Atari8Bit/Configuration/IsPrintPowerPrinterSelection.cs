using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsPrintPowerPrinterSelection(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 7 and <= 32 }
            || data[0] != (byte)'D' || data[1] != (byte)'1' || data[2] != (byte)':')
            return false;
        var terminator = -1;
        for (var index = 3; index < data.Count; index++)
        {
            if (data[index] != 0x9b) continue;
            terminator = index;
            break;
        }
        return terminator is >= 4 and <= 15
            && data.Skip(3).Take(terminator - 3).All(value => value is >= (byte)'0' and <= (byte)'9'
                or >= (byte)'A' and <= (byte)'Z');
    }
}
