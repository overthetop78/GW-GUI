using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;


namespace GWGUI.MediaEngine.Images.Visualization;

internal sealed class CommodoreVisualizationPolicy : SectorImageVisualizationPolicy
{
    public override bool CanHandle(SectorImage image) =>
        image.FormatId.StartsWith(DiskImageFormatIds.CommodorePrefix, StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.StartsWith(DiskImageFormatIds.Commodore900Prefix, StringComparison.OrdinalIgnoreCase);

    public override string EncoderId(SectorImage image)
    {
        if (image.FormatId.StartsWith(DiskImageFormatIds.Commodore1581, StringComparison.OrdinalIgnoreCase)) return FluxCodecIds.IsoMfm;
        if (image.FormatId.StartsWith(DiskImageFormatIds.Commodore900Prefix, StringComparison.OrdinalIgnoreCase)) return FluxCodecIds.Commodore900Gcr;
        return FluxCodecIds.CommodoreGcr;
    }

    public override IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image,
        IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items)
    {
        if (!image.FormatId.StartsWith(DiskImageFormatIds.Commodore1581, StringComparison.OrdinalIgnoreCase))
            return base.CreateTrackSectors(image, items);
        return items.Select(item => item.Block).GroupBy(block => block.LogicalBlock / 2)
            .OrderBy(group => group.Key).Select(group =>
            {
                var halves = group.OrderBy(block => block.LogicalBlock).ToArray();
                var data = halves.SelectMany(block => block.Data).Take(Commodore1581Geometry.PhysicalSectorSize).ToArray();
                return new TrackSector(group.Key % Commodore1581Geometry.PhysicalSectorsPerTrack + 1, data, SizeCode: 2);
            }).Where(sector => sector.Data.Count == Commodore1581Geometry.PhysicalSectorSize).ToArray();
    }

    public override SectorAddress VisualAddress(SectorImage image, SectorAddress address)
    {
        if (!image.FormatId.StartsWith(DiskImageFormatIds.Commodore1581, StringComparison.OrdinalIgnoreCase)) return address;
        var logical = address.Cylinder * image.SectorsPerTrack + address.Number;
        var physical = logical / Commodore1581Geometry.LogicalBlocksPerPhysicalSector;
        var sectorsPerCylinder = Commodore1581Geometry.PhysicalHeadCount * Commodore1581Geometry.PhysicalSectorsPerTrack;
        return new(physical / sectorsPerCylinder, physical % sectorsPerCylinder / Commodore1581Geometry.PhysicalSectorsPerTrack, physical % Commodore1581Geometry.PhysicalSectorsPerTrack + 1);
    }

    public override uint BitCellTicks(SectorImage image, int cylinder)
    {
        if (!image.FormatId.StartsWith(DiskImageFormatIds.Commodore900Prefix, StringComparison.OrdinalIgnoreCase)) return 40;
        return cylinder switch { < Commodore900Geometry.Zone2StartCylinder => 86, < Commodore900Geometry.Zone3StartCylinder => 93, < Commodore900Geometry.Zone4StartCylinder => 100, _ => 106 };
    }
}
