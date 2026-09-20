using GWGUI.MediaEngine.Representations.Sectors;
using FileSystemsAppleDosVolumeWriter = GWGUI.MediaFileSystems.FileSystems.Apple.Dos.AppleDosVolumeWriter;

namespace GWGUI.MediaEngine.Conversion.Migration;

/// <summary>Construit l'image cible depuis les secteurs Apple DOS préparés par MediaFileSystems.</summary>
public sealed class AppleDosMigrationImageBuilder
{
    public SectorImage Create(MigrationPlan plan, string formatId)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var fileSystemPlan = MediaFileSystemsMigrationPlanAdapter.Convert(plan);
        var sectors = new FileSystemsAppleDosVolumeWriter().Create(fileSystemPlan, formatId);
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
}