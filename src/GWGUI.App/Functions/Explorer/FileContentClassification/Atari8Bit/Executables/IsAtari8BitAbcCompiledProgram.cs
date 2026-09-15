using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitAbcCompiledProgram(IReadOnlyList<byte>? data) =>
        data is { Count: >= 128 }
        && data[0] == 0xff && data[1] == 0xff
        && data[2] == 0x00 && data[3] == 0x26
        && data[4] == 0x26 && data[5] == 0x00
        && ContainsAscii(data, "RUNTIME ERROR", 512);
}
