using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsVideo130XeHelpDocument(FileSystemEntry entry)
    {
        var extension = System.IO.Path.GetExtension(entry.Name);
        if (extension.Length != 4
            || !extension.Skip(1).All(char.IsDigit)
            || entry.Content is not { Count: >= 1024 } data
            || ReadUInt16(data, 0) != data.Count - 3
            || data.Skip(3).Take(4).Any(value => value != 0))
            return false;
        var body = data.Skip(7).ToArray();
        return body.Count(value => (value & 0x7f) <= 0x3f) >= body.Length * 0.75;
    }
}
