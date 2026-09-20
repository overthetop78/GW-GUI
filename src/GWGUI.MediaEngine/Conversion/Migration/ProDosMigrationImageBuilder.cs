using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaFileSystems.Contracts;
using FileSystemsProDosVolumeWriter = GWGUI.MediaFileSystems.FileSystems.Apple.ProDos.ProDosVolumeWriter;

namespace GWGUI.MediaEngine.Conversion.Migration;

/// <summary>Crée l'image cible depuis le volume ProDOS construit par MediaFileSystems.</summary>
public sealed class ProDosMigrationImageBuilder
{
    public SectorImage Create(MigrationPlan plan, string formatId)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var (geometry, addresses) = ResolveGeometry(formatId);
        var fileSystemPlan = MediaFileSystemsMigrationPlanAdapter.Convert(plan);
        var sectors = new FileSystemsProDosVolumeWriter().Create(fileSystemPlan, geometry, addresses);
        return new SectorImage(
            sectors.FormatId,
            sectors.BlockSize,
            sectors.Cylinders,
            sectors.Heads,
            sectors.SectorsPerTrack,
            sectors.AvailableBlocks.Select(block => new SectorBlock(
                block.LogicalBlock,
                new SectorAddress(block.Address.Cylinder, block.Address.Head, block.Address.Number),
                block.Data)),
            capacity: sectors.Capacity,
            logicalBlockCount: sectors.LogicalBlockCount);
    }

    private static (MediaSectorGeometry Geometry, IReadOnlyList<MediaSectorAddress> Addresses) ResolveGeometry(string formatId)
    {
        if (formatId.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) ||
            formatId.Equals(DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase))
        {
            var blocksPerTrack = AppleIIGeometry.ProDosBlocksPerTrack;
            var blockCount = AppleIIGeometry.TrackCount * blocksPerTrack;
            return (new(formatId, AppleIIGeometry.ProDosBlockSize, AppleIIGeometry.TrackCount, 1, blocksPerTrack, blockCount),
                Enumerable.Range(0, blockCount).Select(logical => new MediaSectorAddress(logical / blocksPerTrack, 0, logical % blocksPerTrack)).ToArray());
        }
        if (formatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase))
        {
            var heads = MacintoshGcrGeometry.DoubleSidedHeadCount;
            var blockCount = MacintoshGcrGeometry.SingleSidedBlockCount * heads;
            return (new(formatId, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.CylinderCount, heads, MacintoshGcrGeometry.MaximumSectorsPerTrack, blockCount),
                Enumerable.Range(0, blockCount).Select(logical =>
                {
                    var address = MacintoshGcrGeometry.Address(logical, heads);
                    return new MediaSectorAddress(address.Cylinder, address.Head, address.Number);
                }).ToArray());
        }
        throw new ArgumentException($"Unsupported ProDOS target format '{formatId}'.", nameof(formatId));
    }
}
