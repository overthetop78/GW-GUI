using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsFontMakerEditor(IReadOnlyList<byte>? data) =>
        data is { Count: 2668 }
        && ContainsAscii(data, "FontMaker by Charles Brannon", 512)
        && ContainsAscii(data, "Pick a character...", data.Count);
}
