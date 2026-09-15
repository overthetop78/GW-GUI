using System.IO;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAplibAtariCharacterResource(FileSystemEntry entry)
    {
        if (!Path.GetExtension(entry.Name).Equals(".fn", StringComparison.OrdinalIgnoreCase)
            || !TryDecompressAplib(entry.Content, 9 * 1024, out var decoded)
            || decoded.Length is not (1024 or 9 * 1024))
            return false;

        return decoded.Any(value => value != 0x00)
            && decoded.Any(value => value != 0xff);
    }
}
