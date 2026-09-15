using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsUueDecoderBinaryCommand(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".bat", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 250 } data
        && data.Take(4).SequenceEqual(new byte[] { 0x93, 0xc1, 0x15, 0xd1 })
        && data.Skip(data.Count - 5).SequenceEqual(new byte[] { 0x20, 0xd9, 0x30, 0xa0, 0x02 });
}
