using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsSuper3DPlotterLines(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".lin", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { } data)
            return false;
        if (data.Count == 0) return true;
        return data.Count >= 6
            && data.Count % 3 == 0
            && data[0] is >= 1 and <= 7
            && data.Skip(data.Count - 3).All(value => value == 0);
    }
}
