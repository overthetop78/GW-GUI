using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsDosExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: >= 2 } && data[0] == (byte)'M' && data[1] == (byte)'Z';
}
