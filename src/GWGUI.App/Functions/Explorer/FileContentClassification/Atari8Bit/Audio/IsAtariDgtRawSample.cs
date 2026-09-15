using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariDgtRawSample(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".dgt", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 4096 } data
        && data.Distinct().Take(128).Count() == 128;
}
