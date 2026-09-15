using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAngRawDigitizedSample(FileSystemEntry entry)
    {
        if (entry.Content is not { Count: >= 512 and <= 4096 } data
            || !entry.Name.StartsWith("DOCUMENT.", StringComparison.OrdinalIgnoreCase))
            return false;

        var extension = System.IO.Path.GetExtension(entry.Name);
        if (extension.Length <= 1 || !int.TryParse(extension.AsSpan(1), out _)) return false;

        return data.Distinct().Count() >= 48
            && data.Count(value => value == 0) * 4 < data.Count
            && data.Count(value => (value & 0x80) != 0) * 5 >= data.Count * 2;
    }
}
