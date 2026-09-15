using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsEpsonFxDownloadableFont(IReadOnlyList<byte>? data) =>
        data is { Count: >= 8 }
        && data[0] == 0x1b && data[1] == (byte)':'
        && Enumerable.Range(2, Math.Min(32, data.Count - 3))
            .Any(index => data[index] == 0x1b && data[index + 1] == (byte)'&');
}
