using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariAssemblerSource(IReadOnlyList<byte>? data) =>
        data is { Count: >= 128 }
        && ContainsAscii(data, "LDA", 2048)
        && ContainsAscii(data, "STA", 2048)
        && ContainsAscii(data, "JSR", 2048)
        && ContainsAscii(data, "RTS", 2048);
}
