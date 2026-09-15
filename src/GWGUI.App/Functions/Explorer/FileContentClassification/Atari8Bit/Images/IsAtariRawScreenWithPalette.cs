using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariRawScreenWithPalette(IReadOnlyList<byte>? data) =>
        data is { Count: 244 or 7684 or 7685 };
}
