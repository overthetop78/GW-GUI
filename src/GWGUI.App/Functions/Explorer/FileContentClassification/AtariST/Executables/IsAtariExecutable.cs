using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: >= 2 } && data[0] == 0x60 && data[1] == 0x1A;
}
