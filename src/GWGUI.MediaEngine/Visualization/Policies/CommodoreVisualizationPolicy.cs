using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.Commodore;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Visualization;

namespace GWGUI.MediaEngine.Visualization.Policies;

/// <summary>Détermine l'encodage et la géométrie de visualisation des images Commodore.</summary>
internal sealed class CommodoreVisualizationPolicy : SectorImageVisualizationPolicy
{
    /// <inheritdoc />
    public override bool CanHandle(SectorImage image) => image.FormatId.StartsWith(DiskImageFormatIds.CommodorePrefix, StringComparison.OrdinalIgnoreCase) || image.FormatId.StartsWith(DiskImageFormatIds.Commodore900Prefix, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string EncoderId(SectorImage image)
    {
        if (image.FormatId.StartsWith(DiskImageFormatIds.Commodore1581, StringComparison.OrdinalIgnoreCase)) return FluxCodecIds.IsoMfm;
        if (image.FormatId.StartsWith(DiskImageFormatIds.Commodore900Prefix, StringComparison.OrdinalIgnoreCase)) return FluxCodecIds.Commodore900Gcr;
        return FluxCodecIds.CommodoreGcr;
    }

    /// <inheritdoc />
    public override IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image, IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items)
    {
        if (!image.FormatId.StartsWith(DiskImageFormatIds.Commodore1581, StringComparison.OrdinalIgnoreCase))
            return base.CreateTrackSectors(image, items);
        return items.Select(item => item.Block).GroupBy(block => block.LogicalBlock / Commodore1581Geometry.LogicalBlocksPerPhysicalSector)
            .OrderBy(group => group.Key).Select(group =>
            {
                var halves = group.OrderBy(block => block.LogicalBlock).ToArray();
                var data = halves.SelectMany(block => block.Data).Take(Commodore1581Geometry.PhysicalSectorSize).ToArray();
                return new TrackSector(group.Key % Commodore1581Geometry.PhysicalSectorsPerTrack + Commodore1581Geometry.FirstPhysicalSectorNumber, data, SizeCode: Commodore1581Geometry.PhysicalSectorSizeCode);
            }).Where(sector => sector.Data.Count == Commodore1581Geometry.PhysicalSectorSize).ToArray();
    }

    public override SectorAddress VisualAddress(SectorImage image, SectorAddress address)
    {
        if (!image.FormatId.StartsWith(DiskImageFormatIds.Commodore1581, StringComparison.OrdinalIgnoreCase)) return address;
        var logical = address.Cylinder * image.SectorsPerTrack + address.Number;
        var physical = logical / Commodore1581Geometry.LogicalBlocksPerPhysicalSector;
        var sectorsPerCylinder = Commodore1581Geometry.PhysicalHeadCount * Commodore1581Geometry.PhysicalSectorsPerTrack;
        return new(physical / sectorsPerCylinder, physical % sectorsPerCylinder / Commodore1581Geometry.PhysicalSectorsPerTrack, physical % Commodore1581Geometry.PhysicalSectorsPerTrack + Commodore1581Geometry.FirstPhysicalSectorNumber);
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, int>? TrackAttributes(SectorImage image, int sectorCount)
    {
        if (image.FormatId.Equals(DiskImageFormatIds.Commodore1541, StringComparison.OrdinalIgnoreCase) ||
            image.FormatId.Equals(DiskImageFormatIds.Commodore1571, StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int> { [TrackEncodingAttributeKeys.TracksPerSide] = image.Cylinders };
        return null;
    }

    /// <inheritdoc />
    public override uint BitCellTicks(SectorImage image, int cylinder)
    {
        if (image.FormatId.StartsWith(DiskImageFormatIds.Commodore900Prefix, StringComparison.OrdinalIgnoreCase)) return CommodoreTrackEncodingTimings.Commodore900BitCellTicks(cylinder);
        if (image.FormatId.Equals(DiskImageFormatIds.Commodore1541, StringComparison.OrdinalIgnoreCase) ||
            image.FormatId.Equals(DiskImageFormatIds.Commodore1571, StringComparison.OrdinalIgnoreCase)) return CommodoreTrackEncodingTimings.Commodore1541BitCellTicks(cylinder);
        if (image.FormatId.Equals(DiskImageFormatIds.Commodore1581, StringComparison.OrdinalIgnoreCase)) return TrackEncodingTimings.DoubleDensityMfmBitCellTicks;
        return base.BitCellTicks(image, cylinder);
    }
}
