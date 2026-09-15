using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsRambrandtUserPattern(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".usr", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: 424 } data
            || data[0] != 0x0b || data[1] != 0 || data[2] != 0 || data[3] != 0x03)
            return false;

        byte[] signature = [0x9e, 0x7a, 0x62, 0xc4, 0x59, 0x52, 0x4e, 0xc7, 0xf6, 0xb1, 0x07];
        return new[] { 32, 116, 200, 284, 368 }
            .All(offset => data.Skip(offset).Take(signature.Length).SequenceEqual(signature));
    }
}
