using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariRawFontBank(IReadOnlyList<byte>? data) =>
        data is { Count: 3072 }
        && data.Take(8).All(value => value == 0)
        && data.Skip(8).Take(5).All(value => value == 0x38);
}
