using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsPrintShopConverterScreen(IReadOnlyList<byte>? data) =>
        data is { Count: 7857 }
        && data.Take(7680).Any(value => value != 0)
        && data.Skip(7680).Take(176).All(value => value == 0)
        && data[7856] == 0x11;
}
