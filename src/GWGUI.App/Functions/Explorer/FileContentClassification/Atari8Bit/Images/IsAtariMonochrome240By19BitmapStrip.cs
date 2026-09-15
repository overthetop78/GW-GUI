using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariMonochrome240By19BitmapStrip(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".mem", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: 30 * 19 } data)
        {
            return false;
        }

        var hasPixels = false;
        for (var row = 0; row < 19; row++)
        {
            var offset = row * 30;
            for (var column = 0; column < 10; column++)
            {
                if (data[offset + column] != 0 || data[offset + 20 + column] != 0) return false;
            }

            for (var column = 10; column < 20; column++) hasPixels |= data[offset + column] != 0;
        }

        return hasPixels;
    }
}
