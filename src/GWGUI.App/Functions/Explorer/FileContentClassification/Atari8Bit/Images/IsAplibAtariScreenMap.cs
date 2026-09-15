using System.IO;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAplibAtariScreenMap(FileSystemEntry entry) =>
        Path.GetExtension(entry.Name).Equals(".sc", StringComparison.OrdinalIgnoreCase)
        && TryDecompressAplib(entry.Content, 40 * 28, out var decoded)
        && decoded.Length == 40 * 28
        && decoded.All(value => value < 0x80)
        && decoded.Distinct().Skip(1).Any();
}
