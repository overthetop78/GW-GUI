using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsSchematicDesignerDocument(FileSystemEntry entry)
    {
        if (entry.Content is not { } data) return false;
        var extension = System.IO.Path.GetExtension(entry.Name);
        if (string.Equals(extension, ".sch", StringComparison.OrdinalIgnoreCase))
            return data.Count == 7424
                && data.Take(40).All(value => value == 0)
                && data.Skip(data.Count - 32).All(value => value == 0)
                && data.Skip(40).Take(data.Count - 72).Any(value => value != 0);

        return string.Equals(extension, ".zom", StringComparison.OrdinalIgnoreCase)
            && data.Count == 1760
            && data.Take(20).All(value => value == 0)
            && data.Skip(data.Count - 32).All(value => value == 0xff)
            && data.Skip(20).Take(data.Count - 52).Any(value => value != 0);
    }
}
