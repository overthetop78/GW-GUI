using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsSuper3DPlotterPoints(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".pts", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 10 } data
        && data[0] > 0
        && data.Count == 4 + data[0] * 6;
}
