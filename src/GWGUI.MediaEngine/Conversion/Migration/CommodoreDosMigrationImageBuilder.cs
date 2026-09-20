using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaFileSystems.FileSystems.Commodore.Dos;

namespace GWGUI.MediaEngine.Conversion.Migration;

/// <summary>Crée l'image Commodore DOS depuis les secteurs préparés par MediaFileSystems.</summary>
public sealed class CommodoreDosMigrationImageBuilder
{
    /// <summary>Demande au système de fichiers cible de construire les secteurs D64, D71 ou D81.</summary>
    public SectorImage Create(MigrationPlan plan, string formatId, CommodoreDosWritePolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var fileSystemPlan = MediaFileSystemsMigrationPlanAdapter.Convert(plan);
        var sectors = new CommodoreDosVolumeWriter().Create(fileSystemPlan, formatId, policy);
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
