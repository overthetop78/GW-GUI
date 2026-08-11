using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;


namespace GWGUI.MediaEngine.Images.Interpretations;

internal sealed class AtariRecognizedImageNormalizer : IRecognizedImageNormalizer
{
    public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
    {
        normalized = image;
        if (!readerId.Equals("fat12", StringComparison.OrdinalIgnoreCase)) return false;
        if (image.FormatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase) &&
            SectorImageInterpretation.TryReadFatGeometry(image, out var cylinders, out var heads,
                out var sectorsPerTrack, out var totalSectors) && totalSectors < image.BlockCount)
        {
            var blocks = image.AvailableBlocks.Where(block => block.LogicalBlock < totalSectors).ToArray();
            normalized = new(DiskImageFormatIds.AtariStFromCapacity(totalSectors * 512L), 512, cylinders, heads, sectorsPerTrack, blocks,
                capacity: totalSectors * 512L, logicalBlockCount: totalSectors);
            return true;
        }
        if (image.FormatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) &&
            SectorImageInterpretation.ContainsAtariStProgram(volume.Entries))
        {
            normalized = SectorImageInterpretation.Retag(image, DiskImageFormatIds.AtariStFromCapacity(image.Capacity));
            return true;
        }
        return false;
    }
}
