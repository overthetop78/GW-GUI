using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsDigiVoiceSample(IReadOnlyList<byte>? data) =>
        data is { Count: 32518 }
        && ReadUInt16(data, 0) == 0xffff
        && ReadUInt16(data, 2) == 0x4000
        && ReadUInt16(data, 4) == 0xbf00;
}
