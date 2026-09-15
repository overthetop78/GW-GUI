using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariCadVectorDrawing(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".cad", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: >= 16 } data
            || data[0] != (byte)'C' || data[1] != (byte)'A' || data[2] != (byte)'D'
            || data[3] is 0 or > 64 || data[4] != 0 || data[5] != 0)
            return false;

        var coordinateCount = data[3] + 1;
        var tableEnd = 6 + coordinateCount * 3;
        if (tableEnd >= data.Count) return false;
        for (var index = 0; index < coordinateCount; index++)
        {
            if (data[6 + index * 3] != 0x2c) return false;
        }
        return data.Skip(tableEnd).Any(value => value != 0);
    }
}
