using System.IO;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAplibVbxeLowResolutionImage(FileSystemEntry entry) =>
        Path.GetExtension(entry.Name).Equals(".cm", StringComparison.OrdinalIgnoreCase)
        && TryDecompressAplib(entry.Content, 160 * 240, out var decoded)
        && decoded.Length == 160 * 240;
}
