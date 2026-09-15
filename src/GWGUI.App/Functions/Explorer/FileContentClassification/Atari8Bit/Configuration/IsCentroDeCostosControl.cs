using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsCentroDeCostosControl(IReadOnlyList<byte>? data) =>
        data is { Count: 16 }
        && data.Take(7).All(value => value == 2)
        && data.Skip(7).Take(7).All(value => value == 1)
        && data[14] == 0x1d && data[15] == 0x9b;
}
