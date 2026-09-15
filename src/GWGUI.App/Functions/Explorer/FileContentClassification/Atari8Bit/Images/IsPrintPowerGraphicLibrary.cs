using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsPrintPowerGraphicLibrary(FileSystemEntry entry)
    {
        var extension = System.IO.Path.GetExtension(entry.Name);
        if (!(string.Equals(extension, ".001", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".004", StringComparison.OrdinalIgnoreCase))
            || entry.Content is not { Count: >= 256 } data
            || data[1] != 0 || data[2] != 0 || data[3] != 0)
            return false;

        var imageCount = data[0];
        if (imageCount is < 1 or > 64) return false;
        var headerLength = 4 + ((imageCount - 1) * 2);
        if (headerLength >= data.Count) return false;

        var previousEnd = headerLength;
        for (var index = 0; index < imageCount - 1; index++)
        {
            var imageEnd = ReadUInt16(data, 4 + (index * 2));
            if (imageEnd <= previousEnd || imageEnd >= data.Count) return false;
            previousEnd = imageEnd;
        }

        return true;
    }
}
