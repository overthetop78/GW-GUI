using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Interpretation.Contracts;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Interpretation.Normalizers;

/// <summary>Réduit une image Atari ST depuis son BPB ou réidentifie une image IBM contenant un programme Atari.</summary>
internal sealed class AtariRecognizedImageNormalizer : IRecognizedImageNormalizer
{
    /// <summary>Applique l'une des deux règles après reconnaissance FAT12 et conserve l'image source en cas de refus.</summary>
    public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
    {
        normalized = image;
        if (!readerId.Equals(FileSystemIds.Fat12, StringComparison.OrdinalIgnoreCase)) return false;
        if (image.FormatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase) && image.TryGetBlock(FatBootSectorLayout.BootLogicalBlock, out var boot) && FatBpbGeometryDetector.TryDetect(boot.Data.ToArray(), null, out var geometry))
        {
            var blocks = image.AvailableBlocks.Where(block => block.LogicalBlock < geometry.TotalSectors).ToArray();
            var capacity = geometry.TotalSectors * (long)geometry.SectorSize;
            normalized = new(DiskImageFormatIds.AtariStFromCapacity(capacity), geometry.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks, capacity: capacity, logicalBlockCount: geometry.TotalSectors);
            return true;
        }
        return false;
    }
}
