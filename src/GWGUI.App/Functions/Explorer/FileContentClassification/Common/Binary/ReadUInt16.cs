using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static int ReadUInt16(IReadOnlyList<byte> data, int offset) =>
        data[offset] | data[offset + 1] << 8;
}
