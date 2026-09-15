using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsSchemaDesignDocument(FileSystemEntry entry)
    {
        if (!string.IsNullOrEmpty(System.IO.Path.GetExtension(entry.Name))
            || entry.Content is not { } data)
            return false;

        if (data.Count == 6200)
            return data.Take(40).All(value => value == 0)
                && data[40] == 0x7f
                && data.Skip(41).Take(38).All(value => value == 0xff)
                && data[79] == 0xfe
                && data.Skip(6120).All(value => value == 0);

        return data.Count == 4750
            && data.Take(64).All(value => value == 0)
            && data[87] == 0xc0 && data[88] == 0 && data[89] == 0x60
            && data.Skip(4089).All(value => value == 0);
    }
}
