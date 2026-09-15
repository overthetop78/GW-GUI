using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsTechnicolorDreamImageComponent(FileSystemEntry entry)
    {
        var extension = System.IO.Path.GetExtension(entry.Name);
        return (string.Equals(extension, ".col", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".lum", StringComparison.OrdinalIgnoreCase))
            && entry.Content is { Count: >= 128 } data
            && data[2] == 0x16 && data[3] == 0x05 && data[4] == 0x35 && data[5] == 0x00;
    }
}
