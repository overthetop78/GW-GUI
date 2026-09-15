using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariPageSeparatedRasterControlTable(FileSystemEntry entry)
    {
        var extension = System.IO.Path.GetExtension(entry.Name);
        if (!(string.Equals(extension, ".srt", StringComparison.OrdinalIgnoreCase)
              || string.Equals(extension, ".trs", StringComparison.OrdinalIgnoreCase))
            || entry.Content is not { Count: 3 * 256 } data)
        {
            return false;
        }

        var lastFirstPageValue = LastNonZeroIndex(data, 0);
        var lastSecondPageValue = LastNonZeroIndex(data, 256);
        if (lastFirstPageValue <= 0 || lastFirstPageValue != lastSecondPageValue || lastFirstPageValue >= 254)
        {
            return false;
        }

        var hasControlValue = false;
        for (var index = 0; index <= lastFirstPageValue; index++)
        {
            var control = data[512 + index];
            if (control > 0x08) return false;
            hasControlValue |= control != 0;
        }

        var terminatorIndex = lastFirstPageValue + 1;
        if (!hasControlValue || data[512 + terminatorIndex] != 0xff) return false;
        for (var index = terminatorIndex + 1; index < 256; index++)
        {
            if (data[512 + index] != 0) return false;
        }

        return true;
    }

    private static int LastNonZeroIndex(IReadOnlyList<byte> data, int offset)
    {
        for (var index = 255; index >= 0; index--)
        {
            if (data[offset + index] != 0) return index;
        }

        return -1;
    }
}
