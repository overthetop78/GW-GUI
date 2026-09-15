using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsScreenAidedManagementCatalog(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".mem", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 3072 } data
        && data.Take(13).Any(value => value != 0)
        && data.Skip(13).All(value => value == 0);
}
