using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsPlayerMissileGraphicsTabletImage(IReadOnlyList<byte>? data) =>
        data is { Count: 4206 }
        && data[0] == 0x17
        && data.Skip(1).Take(5).All(value => (value & 1) == 0);
}
