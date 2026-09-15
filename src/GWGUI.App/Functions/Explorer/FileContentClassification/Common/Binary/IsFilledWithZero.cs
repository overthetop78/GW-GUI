using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsFilledWithZero(IReadOnlyList<byte>? data) =>
        data is { Count: > 0 } && data.All(value => value == 0);
}
