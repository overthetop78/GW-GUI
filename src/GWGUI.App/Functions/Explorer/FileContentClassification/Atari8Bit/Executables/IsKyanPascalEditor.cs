using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsKyanPascalEditor(IReadOnlyList<byte>? data) =>
        data is { Count: >= 512 }
        && ReadUInt16(data, 0) == 0xffff
        && ReadUInt16(data, 2) == 0x2005
        && ReadUInt16(data, 4) == 0x342a
        && ContainsAscii(data, "Not a Load File", data.Count)
        && ContainsAscii(data, "Invailid Device", data.Count);
}
