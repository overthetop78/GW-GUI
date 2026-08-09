using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Interpretations;

internal sealed class AtariRecognizedImageNormalizer : IRecognizedImageNormalizer
{
    public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
    {
        normalized = image;
        if (!readerId.Equals("fat12", StringComparison.OrdinalIgnoreCase)) return false;
        if (image.FormatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase) &&
            SectorImageInterpretation.TryReadFatGeometry(image, out var cylinders, out var heads,
                out var sectorsPerTrack, out var totalSectors) && totalSectors < image.BlockCount)
        {
            var blocks = image.AvailableBlocks.Where(block => block.LogicalBlock < totalSectors).ToArray();
            normalized = new($"atarist.{totalSectors / 2}", 512, cylinders, heads, sectorsPerTrack, blocks,
                capacity: totalSectors * 512L, logicalBlockCount: totalSectors);
            return true;
        }
        if (image.FormatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) &&
            SectorImageInterpretation.ContainsAtariStProgram(volume.Entries))
        {
            normalized = SectorImageInterpretation.Retag(image, $"atarist.{image.Capacity / 1024}");
            return true;
        }
        return false;
    }
}
