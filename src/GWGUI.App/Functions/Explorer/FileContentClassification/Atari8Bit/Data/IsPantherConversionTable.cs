using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsPantherConversionTable(FileSystemEntry entry) =>
        entry.Content is { Count: > 0 } data
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".cvn", StringComparison.OrdinalIgnoreCase)
        && data[0] == 0x9b
        && ContainsAscii(data, "#13=", data.Count)
        && ContainsAscii(data, "#10=", data.Count);
}
