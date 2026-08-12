using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Normalise une image FAT reconnue vers son identité Atari ST lorsque les données le prouvent.</summary>
internal sealed class AtariRecognizedImageNormalizer : IRecognizedImageNormalizer
{
    /// <inheritdoc />
    public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
    {
        normalized = image;
        if (!readerId.Equals(FileSystemIds.Fat12, StringComparison.OrdinalIgnoreCase)) return false;
        if (image.FormatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase) &&
            SectorImageInterpretation.TryReadFatGeometry(image, out var cylinders, out var heads, out var sectorsPerTrack, out var totalSectors) && totalSectors < image.BlockCount)
        {
            var blocks = image.AvailableBlocks.Where(block => block.LogicalBlock < totalSectors).ToArray();
            var capacity = totalSectors * (long)FatBootSectorLayout.SectorSize;
            normalized = new(DiskImageFormatIds.AtariStFromCapacity(capacity), FatBootSectorLayout.SectorSize, cylinders, heads, sectorsPerTrack, blocks, capacity: capacity, logicalBlockCount: totalSectors);
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
