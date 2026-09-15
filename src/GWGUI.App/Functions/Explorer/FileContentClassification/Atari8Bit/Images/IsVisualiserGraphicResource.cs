using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsVisualiserGraphicResource(IReadOnlyList<byte>? data) =>
        data is { Count: 3165 }
            && data.Take(6).SequenceEqual(new byte[] { 0x2b, 0x28, 0x01, 0x00, 0xa4, 0x00 })
        || data is { Count: 3328 }
            && data.Take(8).SequenceEqual(new byte[] { 0x66, 0x66, 0x66, 0x36, 0x00, 0x00, 0x00, 0xf0 });
}
