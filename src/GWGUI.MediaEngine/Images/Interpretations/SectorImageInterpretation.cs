using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;


namespace GWGUI.MediaEngine.Images.Interpretations;

internal static class SectorImageInterpretation
{
    public static SectorImage Retag(SectorImage image, string formatId) => new(formatId, image.BlockSize,
        image.Cylinders, image.Heads, image.SectorsPerTrack, image.AvailableBlocks,
        image.AvailableBlocks.Any(block => block.Data.Count != image.BlockSize), image.Capacity, image.BlockCount);

    public static bool ContainsAtariStProgram(IEnumerable<FileSystemEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Kind == FileSystemEntryKind.File)
            {
                var extension = Path.GetExtension(entry.Name);
                if (extension.Equals(".ttp", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".tos", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".acc", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".gtp", StringComparison.OrdinalIgnoreCase) ||
                    entry.Content is { Count: >= 2 } && entry.Content[0] == 0x60 && entry.Content[1] == 0x1a)
                    return true;
            }
            if (ContainsAtariStProgram(entry.Children)) return true;
        }
        return false;
    }

    public static bool TryReadFatGeometry(SectorImage image, out int cylinders, out int heads,
        out int sectorsPerTrack, out int totalSectors)
    {
        cylinders = heads = sectorsPerTrack = totalSectors = 0;
        if (image.BlockSize != 512 || !image.TryGetBlock(0, out var boot) || boot.Data.Count < 36) return false;
        var bytes = boot.Data;
        var bytesPerSector = bytes[11] | bytes[12] << 8;
        totalSectors = bytes[19] | bytes[20] << 8;
        if (totalSectors == 0)
            totalSectors = bytes[32] | bytes[33] << 8 | bytes[34] << 16 | bytes[35] << 24;
        sectorsPerTrack = bytes[24] | bytes[25] << 8;
        heads = bytes[26] | bytes[27] << 8;
        if (bytesPerSector != 512 || totalSectors <= 0 || sectorsPerTrack <= 0 || heads <= 0 ||
            totalSectors > image.BlockCount || totalSectors % (sectorsPerTrack * heads) != 0)
            return false;
        cylinders = totalSectors / (sectorsPerTrack * heads);
        return cylinders > 0;
    }

    public static bool TryCreateMsx(SectorImage image, out SectorImage interpretation)
    {
        interpretation = null!;
        if (image.FormatId.StartsWith(DiskImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase) ||
            !image.TryGetBlock(0, out var boot) || boot.Data.Count != 512 ||
            !MsxImageReader.LooksLikeMsx(boot.Data.ToArray()))
            return false;
        var formatId = image.BlockCount switch
        {
            360 => DiskImageFormatIds.Msx1D,
            720 when boot.Data.Count > 21 && boot.Data[21] == 0xf8 => DiskImageFormatIds.Msx1Dd,
            720 => DiskImageFormatIds.Msx2D,
            1440 => DiskImageFormatIds.Msx2Dd,
            _ => string.Empty
        };
        if (formatId.Length == 0) return false;
        interpretation = Retag(image, formatId);
        return true;
    }
}
