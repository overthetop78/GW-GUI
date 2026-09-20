using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaEngine.Formats.Floppy.Adf;
using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaFileSystems.Contracts;
using FileSystemsAmiga = GWGUI.MediaFileSystems.FileSystems.Amiga;

namespace GWGUI.MediaEngine.Conversion.Migration;

/// <summary>Construit une image ADF à partir du plan de secteurs préparé par MediaFileSystems.</summary>
public sealed class AmigaDosMigrationImageBuilder
{
    public SectorImage Create(MigrationPlan plan, AmigaDosVariant variant, string formatId = DiskImageFormatIds.AmigaDos)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var geometry = formatId.Equals(DiskImageFormatIds.AmigaDos, StringComparison.OrdinalIgnoreCase)
            ? AmigaAdfGeometry.DoubleDensity
            : formatId.Equals(DiskImageFormatIds.AmigaDosHighDensity, StringComparison.OrdinalIgnoreCase)
                ? AmigaAdfGeometry.HighDensity
                : throw AmigaDosVolumeWriterExceptions.UnsupportedGeometry(formatId);

        var fileSystemPlan = MediaFileSystemsMigrationPlanAdapter.Convert(plan);
        var sectorGeometry = new MediaSectorGeometry(
            geometry.FormatId,
            geometry.BlockSize,
            geometry.Cylinders,
            geometry.Heads,
            geometry.SectorsPerTrack,
            geometry.BlockCount);
        var sectors = new FileSystemsAmiga.AmigaDosVolumeWriter().Create(
            fileSystemPlan,
            (FileSystemsAmiga.AmigaDosVariant)variant,
            sectorGeometry);
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
