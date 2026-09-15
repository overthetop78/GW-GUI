using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariMonochrome80By48Bitmap(FileSystemEntry entry)
    {
        if (entry.Content is not { Count: 10 * 48 }) return false;

        var extension = System.IO.Path.GetExtension(entry.Name);
        return string.Equals(extension, ".scn", StringComparison.OrdinalIgnoreCase)
               || extension.Length == 4
               && extension.StartsWith(".sc", StringComparison.OrdinalIgnoreCase)
               && extension[3] is >= '1' and <= '9';
    }
}
