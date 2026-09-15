using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariBasicBlockLoadedLogo(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".fun", StringComparison.OrdinalIgnoreCase)
            || !System.IO.Path.GetFileNameWithoutExtension(entry.Name).Contains("LOGO", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: 160 or 4096 } data)
            return false;

        return data.Count(value => value == 0) >= data.Count / 8
            && data.Distinct().Take(16).Count() == 16;
    }
}
