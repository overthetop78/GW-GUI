using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitRaytracerBackground(IReadOnlyList<byte>? data) =>
        data is { Count: 965 }
        && data[0] == 0x0a && data[1] == 0x06
        && data[2] == 0x0f && data[3] == 0x46;
}
