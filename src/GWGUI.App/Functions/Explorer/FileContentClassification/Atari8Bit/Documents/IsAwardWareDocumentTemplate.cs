using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAwardWareDocumentTemplate(IReadOnlyList<byte>? data)
    {
        if (data is null) return false;
        if (data.Count == 103)
            return data[0] == 0xc1 && data[1] == 0x88
                && data[2] == 0x54 && data[3] == 0x00
                && ContainsAscii(data, "License", data.Count);
        return data.Count is 2432 or 2944 or 3200 or 4736 or 7168
            && ReadUInt16(data, 0) > 0
            && ReadUInt16(data, 0) < data.Count;
    }
}
