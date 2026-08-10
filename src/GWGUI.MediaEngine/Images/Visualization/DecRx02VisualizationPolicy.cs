using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.SectorImages;

using GWGUI.MediaEngine.Recognition.Definitions;

namespace GWGUI.MediaEngine.Images.Visualization;

internal sealed class DecRx02VisualizationPolicy : SectorImageVisualizationPolicy
{
    public override bool CanHandle(SectorImage image) =>
        image.FormatId.Equals(DiskImageFormatIds.DecRx02, StringComparison.OrdinalIgnoreCase);

    public override string EncoderId(SectorImage image) => "dec.rx02";

    public override IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image,
        IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items)
    {
        var sectors = new List<TrackSector>(items.Count * 2);
        foreach (var item in items)
        {
            if (item.Block.Data.Count < 512) continue;
            var first = (item.Address.Number - 1) * 2 + 1;
            sectors.Add(new(first, item.Block.Data.Take(256).ToArray(), SizeCode: 1));
            sectors.Add(new(first + 1, item.Block.Data.Skip(256).Take(256).ToArray(), SizeCode: 1));
        }
        return sectors;
    }
}
