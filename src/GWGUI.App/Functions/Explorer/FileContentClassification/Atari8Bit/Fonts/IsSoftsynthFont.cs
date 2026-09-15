using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsSoftsynthFont(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".syn", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 1568 } data
        && data.Take(10).SequenceEqual(new byte[] { 0x70, 0x70, 0x42, 0x40, 0x9c, 0x02, 0x02, 0x4f, 0x00, 0x70 })
        && data.Skip(10).Take(38).All(value => value == 0x0f)
        && data.Skip(64).Any(value => value != 0);
}
