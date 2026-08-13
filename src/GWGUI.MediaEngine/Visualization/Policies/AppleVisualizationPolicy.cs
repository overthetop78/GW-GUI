using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.Apple;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Visualization;

namespace GWGUI.MediaEngine.Visualization.Policies;

/// <summary>Détermine l'encodage et la géométrie de visualisation des images Apple.</summary>
internal sealed class AppleVisualizationPolicy : SectorImageVisualizationPolicy
{
    /// <inheritdoc />
    public override bool CanHandle(SectorImage image) => image.FormatId.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase) || image.FormatId.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase) || image.FormatId.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) || image.FormatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) || image.FormatId.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string EncoderId(SectorImage image)
    {
        if (image.FormatId.Equals(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase))
            return FluxCodecIds.AppleRwts18;
        if ((image.FormatId.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) ||
             image.FormatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase)) &&
            image.BlockSize == AppleIIGeometry.ProDosBlockSize && image.Cylinders >= DiskGeometryConstants.EightyTrackCylinderCount) return FluxCodecIds.AppleMacGcr;
        if (image.FormatId.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase) ||
            image.FormatId.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase)) return FluxCodecIds.AppleIIGcr;
        if (image.FormatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) &&
            image.Cylinders == LisaFileWareGeometry.CylinderCount && image.Heads == LisaFileWareGeometry.HeadCount) return FluxCodecIds.AppleLisaFileWareGcr;
        if (image.FormatId.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase)) return FluxCodecIds.IsoMfm;
        return FluxCodecIds.AppleMacGcr;
    }

    /// <inheritdoc />
    public override IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image, IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items)
    {
        if ((image.FormatId.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) ||
             image.FormatId.Equals(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase)) &&
            image.Cylinders < DiskGeometryConstants.EightyTrackCylinderCount)
        {
            var sectors = new List<TrackSector>(items.Count * AppleIIGeometry.SectorsPerProDosBlock);
            foreach (var block in items.Select(item => item.Block))
            {
                if (block.Data.Count < AppleIIGeometry.ProDosBlockSize) continue;
                sectors.Add(new(block.Address.Number * AppleIIGeometry.SectorsPerProDosBlock, block.Data.Take(AppleIIGeometry.SectorSize).ToArray()));
                sectors.Add(new(block.Address.Number * AppleIIGeometry.SectorsPerProDosBlock + 1, block.Data.Skip(AppleIIGeometry.SectorSize).Take(AppleIIGeometry.SectorSize).ToArray()));
            }
            return sectors;
        }
        return base.CreateTrackSectors(image, items);
    }

    /// <inheritdoc />
    public override SectorAddress VisualAddress(SectorImage image, SectorAddress address) => image.FormatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) && image.Heads == DiskGeometryConstants.SingleSidedHeadCount && image.Cylinders > LisaFileWareGeometry.LinearTrackThreshold
            ? new(address.Cylinder / LisaFileWareGeometry.HeadCount, address.Cylinder % LisaFileWareGeometry.HeadCount, address.Number)
            : address;

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, int>? TrackAttributes(SectorImage image, int sectorCount)
    {
        if (image.FormatId.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int>
            {
                [TrackEncodingAttributeKeys.SectorsPerTrack] = sectorCount,
                [TrackEncodingAttributeKeys.Format] = image.Cylinders >= DiskGeometryConstants.EightyTrackCylinderCount ? AppleTrackFormatCodes.AppleIIProDos80Track : AppleTrackFormatCodes.AppleII
            };
        if (image.FormatId.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) || image.FormatId.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int> { [TrackEncodingAttributeKeys.Format] = image.Heads == DiskGeometryConstants.SingleSidedHeadCount ? AppleTrackFormatCodes.MacintoshSingleSided : AppleTrackFormatCodes.MacintoshDoubleSided };
        if (image.FormatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int> { [TrackEncodingAttributeKeys.Format] = AppleTrackFormatCodes.LisaFileWare };
        return null;
    }

    /// <inheritdoc />
    public override uint BitCellTicks(SectorImage image, int cylinder)
    {
        var encoderId = EncoderId(image);
        if (encoderId.Equals(FluxCodecIds.AppleIIGcr, StringComparison.OrdinalIgnoreCase) || encoderId.Equals(FluxCodecIds.AppleRwts18, StringComparison.OrdinalIgnoreCase)) return AppleTrackEncodingTimings.AppleIIBitCellTicks;
        if (encoderId.Equals(FluxCodecIds.AppleMacGcr, StringComparison.OrdinalIgnoreCase) || encoderId.Equals(FluxCodecIds.AppleLisaFileWareGcr, StringComparison.OrdinalIgnoreCase)) return AppleTrackEncodingTimings.IwmGcrBitCellTicks;
        return base.BitCellTicks(image, cylinder);
    }

    /// <inheritdoc />
    public override uint IndexTimeTicks(SectorImage image, int cylinder)
    {
        var encoderId = EncoderId(image);
        if (encoderId.Equals(FluxCodecIds.AppleLisaFileWareGcr, StringComparison.OrdinalIgnoreCase)) return AppleTrackEncodingTimings.LisaIndexTimeTicks(cylinder);
        if (encoderId.Equals(FluxCodecIds.AppleMacGcr, StringComparison.OrdinalIgnoreCase)) return AppleTrackEncodingTimings.MacintoshIndexTimeTicks(cylinder);
        return base.IndexTimeTicks(image, cylinder);
    }
}
