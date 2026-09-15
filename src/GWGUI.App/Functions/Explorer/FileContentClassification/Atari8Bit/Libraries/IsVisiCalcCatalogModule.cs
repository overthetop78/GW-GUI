using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsVisiCalcCatalogModule(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".cat", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 375 } data
        && data.Take(8).SequenceEqual(new byte[] { 0xaa, 0x08, 0x14, 0x0b, 0xbe, 0x0a, 0xcb, 0x09 })
        && data.Skip(data.Count - 6).SequenceEqual(new byte[] { 0xa9, 0x20, 0x99, 0x03, 0x14, 0xc8 });
}
