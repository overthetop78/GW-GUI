using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsVideoScannerImage(FileSystemEntry entry) =>
        string.IsNullOrEmpty(System.IO.Path.GetExtension(entry.Name))
        && entry.Content is { Count: 4375 } data
        && data.Take(97).All(value => value == 0)
        && data[97] == 0xc0
        && data[4347] == 0xc0
        && data.Skip(4348).All(value => value == 0)
        && data.Skip(98).Take(4249).Any(value => value != 0);
}
