using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsSuper3DPlotterPrinterProfile(IReadOnlyList<byte>? data) =>
        data is { Count: 18 }
        && data.SequenceEqual(new byte[]
        {
            0x1b, (byte)'@', 0x9b, 0x1b, (byte)'l', 0x00,
            0x1b, (byte)'A', 0x08, 0x9b, 0x1b, (byte)'K', 0xc0, 0x00,
            0x9b, 0x00, 0x00, 0x9b
        });
}
