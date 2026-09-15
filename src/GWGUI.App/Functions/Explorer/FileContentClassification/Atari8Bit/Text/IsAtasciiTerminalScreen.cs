using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtasciiTerminalScreen(FileSystemEntry entry) =>
        entry.Content is { Count: > 0 } data
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".ata", StringComparison.OrdinalIgnoreCase)
        && data.Contains((byte)0x9b);
}
