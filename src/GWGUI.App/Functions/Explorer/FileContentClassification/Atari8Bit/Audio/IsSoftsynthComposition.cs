using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsSoftsynthComposition(IReadOnlyList<byte>? data) =>
        data is { Count: >= 128 }
        && data[0] == (byte)'S' && data[1] == (byte)'Y' && data[2] == (byte)'N' && data[3] == 0x9b
        && data.Skip(data.Count - 5).SequenceEqual(new byte[] { 0x00, 0x80, 0x20, 0x20, 0x20 });
}
