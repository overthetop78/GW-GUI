using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsVirtuosoComposition(FileSystemEntry entry)
    {
        var extension = System.IO.Path.GetExtension(entry.Name);
        return (string.Equals(extension, ".jl", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".jm", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".dm", StringComparison.OrdinalIgnoreCase))
            && entry.Content is { Count: 5875 or 6500 } data
            && data.Take(128).Count(value => value == 0) >= 24
            && data.Distinct().Take(128).Count() == 128;
    }
}
