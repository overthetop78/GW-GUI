using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariGraphicsMemoryPlane(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".mem", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 2560 and <= 7680 } data
        && data.Count % 40 == 0
        && data.Count(value => value == 0) >= data.Count / 16
        && data.Distinct().Take(16).Count() == 16;
}
