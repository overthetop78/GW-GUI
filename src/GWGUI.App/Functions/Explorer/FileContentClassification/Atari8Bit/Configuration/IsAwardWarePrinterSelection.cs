using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAwardWarePrinterSelection(IReadOnlyList<byte>? data) =>
        data is { Count: 16 }
        && data[0] is >= (byte)'0' and <= (byte)'9'
        && data[1] == (byte)':'
        && data.Contains((byte)0x9b);
}
