using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsPrintPowerCatalog(FileSystemEntry entry) =>
        entry.Content is { Count: 593 or 995 } data
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".d8a", StringComparison.OrdinalIgnoreCase)
        && data.Count(value => value == 0x9b) >= 4;
}
