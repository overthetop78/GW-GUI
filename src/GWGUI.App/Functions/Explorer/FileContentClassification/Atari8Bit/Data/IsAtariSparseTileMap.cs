using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariSparseTileMap(IReadOnlyList<byte>? data) =>
        data is { Count: 1000 }
        && data.Count(value => value == 0) >= 400
        && data.Distinct().Take(33).Count() <= 32
        && data.Any(value => value != 0);
}
