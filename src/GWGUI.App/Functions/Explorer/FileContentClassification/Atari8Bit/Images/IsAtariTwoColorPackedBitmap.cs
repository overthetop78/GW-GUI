using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariTwoColorPackedBitmap(IReadOnlyList<byte>? data) =>
        data is { Count: 3072 }
        && data.All(value => ((value ^ (value >> 1)) & 0x55) == 0);
}
