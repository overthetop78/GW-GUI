using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsTrzmielCompressedImage(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".cpr", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 128 } data
        && data[0] == 0x02
        && data.Take(Math.Min(128, data.Count)).Any(value => value is >= 0x82 and <= 0x8f);
}
