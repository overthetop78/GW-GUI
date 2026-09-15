using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAaEditorDocument(IReadOnlyList<byte>? data) =>
        data is { Count: 1012 }
        && data.Take(11).SequenceEqual(new byte[]
        {
            0xa0, 0xc1, 0xc1, 0xc5, 0xe4, 0xe9, 0xf4, 0xef, 0xf2, 0xa0, 0x9b
        });
}
