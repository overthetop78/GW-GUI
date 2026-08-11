using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.SectorImages;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Images.Visualization;

internal sealed class AppleVisualizationPolicy : SectorImageVisualizationPolicy
{
    public override bool CanHandle(SectorImage image) =>
        image.FormatId.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase);

    public override string EncoderId(SectorImage image)
    {
        if (image.FormatId.Equals(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase))
            return FluxCodecIds.AppleRwts18;
        if (image.FormatId.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) &&
            image.BlockSize == 512 && image.Cylinders >= DiskGeometryConstants.EightyTrackCylinderCount) return FluxCodecIds.AppleMacGcr;
        if (image.FormatId.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase) ||
            image.FormatId.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase)) return FluxCodecIds.AppleIIGcr;
        if (image.FormatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) &&
            image.Cylinders == AppleDiskGeometry.LisaFileWareCylinderCount && image.Heads == AppleDiskGeometry.LisaFileWareHeadCount) return FluxCodecIds.AppleLisaFileWareGcr;
        if (image.FormatId.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase)) return FluxCodecIds.IsoMfm;
        return FluxCodecIds.AppleMacGcr;
    }

    public override IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image,
        IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items)
    {
        if ((image.FormatId.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) ||
             image.FormatId.Equals(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase)) &&
            image.Cylinders < DiskGeometryConstants.EightyTrackCylinderCount)
        {
            var sectors = new List<TrackSector>(items.Count * 2);
            foreach (var block in items.Select(item => item.Block))
            {
                if (block.Data.Count < 512) continue;
                sectors.Add(new(block.Address.Number * 2, block.Data.Take(256).ToArray()));
                sectors.Add(new(block.Address.Number * 2 + 1, block.Data.Skip(256).Take(256).ToArray()));
            }
            return sectors;
        }
        return base.CreateTrackSectors(image, items);
    }

    public override SectorAddress VisualAddress(SectorImage image, SectorAddress address) =>
        image.FormatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) &&
        image.Heads == DiskGeometryConstants.SingleSidedHeadCount && image.Cylinders > 84
            ? new(address.Cylinder / 2, address.Cylinder % 2, address.Number)
            : address;

    public override IReadOnlyDictionary<string, int>? TrackAttributes(SectorImage image, int sectorCount)
    {
        if (image.FormatId.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int>
            {
                ["sectorsPerTrack"] = sectorCount,
                ["format"] = image.Cylinders >= DiskGeometryConstants.EightyTrackCylinderCount ? 0x24 : 0
            };
        if (image.FormatId.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int> { ["format"] = image.Heads == DiskGeometryConstants.SingleSidedHeadCount ? 0x02 : 0x22 };
        if (image.FormatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int> { ["format"] = 0x12 };
        return null;
    }
}
