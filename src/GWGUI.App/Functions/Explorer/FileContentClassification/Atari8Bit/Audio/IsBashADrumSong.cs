using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsBashADrumSong(IReadOnlyList<byte>? data) =>
        data is { Count: 40 }
        && data.Take(10).SequenceEqual(Enumerable.Range(1, 10).Select(value => (byte)value));
}
