using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariFortyColumnScreenMemory(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".mem", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 40 * 24 };
}
