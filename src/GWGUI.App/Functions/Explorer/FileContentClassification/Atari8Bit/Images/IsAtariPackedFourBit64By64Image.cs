using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private const int PackedFourBit64By64RowLength = 32;
    private const int PackedFourBit64By64Height = 64;
    private const int PackedFourBit64By64Length = PackedFourBit64By64RowLength * PackedFourBit64By64Height;

    private static bool IsAtariPackedFourBit64By64Image(FileSystemEntry entry)
    {
        if (!System.IO.Path.GetExtension(entry.Name).Equals(".rys", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: PackedFourBit64By64Length } data)
            return false;

        var colorIndexes = data.SelectMany(value => new[] { value >> 4, value & 0x0f });
        var populatedRows = Enumerable.Range(0, PackedFourBit64By64Height)
            .Count(row => data.Skip(row * PackedFourBit64By64RowLength)
                .Take(PackedFourBit64By64RowLength)
                .Any(value => value != 0));
        return colorIndexes.Distinct().Take(4).Count() == 4 && populatedRows >= 8;
    }
}
