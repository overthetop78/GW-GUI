using System.IO;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAplibFixedWidthTextMetadata(FileSystemEntry entry) =>
        Path.GetExtension(entry.Name).Equals(".tx", StringComparison.OrdinalIgnoreCase)
        && TryDecompressAplib(entry.Content, 122, out var decoded)
        && decoded.Length == 122
        && decoded.Take(120).All(value => value is >= 0x20 and <= 0x7e)
        && decoded[121] is (byte)'H' or (byte)'L';
}
