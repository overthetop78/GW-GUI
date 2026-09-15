using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsMusicStudioFile(IReadOnlyList<byte>? data) =>
        data is { Count: >= 256 }
        && ContainsAscii(data, "larinet", 256)
        && ContainsAscii(data, "ass", 256);
}
