using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAwardWareFontWidths(IReadOnlyList<byte>? data) =>
        data is { Count: 455 }
        && data.Take(96).All(value => value <= 0x20);
}
