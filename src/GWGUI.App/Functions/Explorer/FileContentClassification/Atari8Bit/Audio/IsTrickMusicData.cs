using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsTrickMusicData(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".dan", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 1056 } data
        && data.Take(6).SequenceEqual(new byte[] { 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x00 })
        && data.Skip(data.Count - 16).All(value => value == 0xf2);
}
