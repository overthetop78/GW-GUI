using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsSuperMailerPrinterType(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".typ", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 4 } data
        && data.SequenceEqual(new byte[] { 0x01, 0x0f, 0x12, 0x0e });
}
