using GWGUI.Scp.Encoding;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Visualization;

internal sealed class AppleVisualizationPolicy : SectorImageVisualizationPolicy
{
    public override bool CanHandle(SectorImage image) =>
        image.FormatId.StartsWith("apple2.", StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.StartsWith("apple3.", StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.StartsWith("applemac.", StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.StartsWith("applelisa.", StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.StartsWith("mac.", StringComparison.OrdinalIgnoreCase);

    public override string EncoderId(SectorImage image)
    {
        if (image.FormatId.Equals("apple2.rwts18", StringComparison.OrdinalIgnoreCase))
            return "apple2.rwts18";
        if (image.FormatId.Equals("apple2.prodos", StringComparison.OrdinalIgnoreCase) &&
            image.BlockSize == 512 && image.Cylinders >= 80) return "applemac.gcr";
        if (image.FormatId.StartsWith("apple2.", StringComparison.OrdinalIgnoreCase) ||
            image.FormatId.StartsWith("apple3.", StringComparison.OrdinalIgnoreCase)) return "apple2.gcr";
        if (image.FormatId.StartsWith("applelisa.", StringComparison.OrdinalIgnoreCase) &&
            image.Cylinders == 46 && image.Heads == 2) return "applelisa.fileware.gcr";
        if (image.FormatId.Equals("mac.1440", StringComparison.OrdinalIgnoreCase)) return "iso.mfm";
        return "applemac.gcr";
    }

    public override IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image,
        IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items)
    {
        if ((image.FormatId.Equals("apple2.prodos", StringComparison.OrdinalIgnoreCase) ||
             image.FormatId.Equals("apple3.sos", StringComparison.OrdinalIgnoreCase)) &&
            image.Cylinders < 80)
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
        image.FormatId.StartsWith("applelisa.", StringComparison.OrdinalIgnoreCase) &&
        image.Heads == 1 && image.Cylinders > 84
            ? new(address.Cylinder / 2, address.Cylinder % 2, address.Number)
            : address;

    public override IReadOnlyDictionary<string, int>? TrackAttributes(SectorImage image, int sectorCount)
    {
        if (image.FormatId.StartsWith("apple2.", StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int>
            {
                ["sectorsPerTrack"] = sectorCount,
                ["format"] = image.Cylinders >= 80 ? 0x24 : 0
            };
        if (image.FormatId.StartsWith("applemac.", StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int> { ["format"] = image.Heads == 1 ? 0x02 : 0x22 };
        if (image.FormatId.StartsWith("applelisa.", StringComparison.OrdinalIgnoreCase))
            return new Dictionary<string, int> { ["format"] = 0x12 };
        return null;
    }
}
