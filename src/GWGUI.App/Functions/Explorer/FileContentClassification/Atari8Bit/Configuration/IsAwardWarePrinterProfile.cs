using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAwardWarePrinterProfile(IReadOnlyList<byte>? data) =>
        data is { Count: 291 or 292 }
        && data.Take(32).Contains((byte)0x0d)
        && data.Take(32).Contains((byte)0x0a);
}
