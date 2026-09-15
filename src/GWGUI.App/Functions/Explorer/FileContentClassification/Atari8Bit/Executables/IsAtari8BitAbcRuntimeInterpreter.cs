using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitAbcRuntimeInterpreter(IReadOnlyList<byte>? data) =>
        data is { Count: 5131 }
        && ReadUInt16(data, 0) == 0xffff
        && ReadUInt16(data, 2) == 0x2600
        && ReadUInt16(data, 4) == 0x37c5
        && ContainsAscii(data, "RUNTIME ERROR", 512);
}
