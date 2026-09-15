using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsRubberStampPad(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".pad", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 1791 } data
        && data.Take(256).All(value => value == 0)
        && data.Skip(256).Take(data.Count - 512).Any(value => value != 0)
        && data.Skip(data.Count - 256).All(value => value == 0);
}
