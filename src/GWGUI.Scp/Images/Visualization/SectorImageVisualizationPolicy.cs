using GWGUI.Scp.Encoding;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Visualization;

internal abstract class SectorImageVisualizationPolicy : ISectorImageVisualizationPolicy
{
    public abstract bool CanHandle(SectorImage image);
    public abstract string EncoderId(SectorImage image);
    public virtual SectorAddress VisualAddress(SectorImage image, SectorAddress address) => address;

    public virtual IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image,
        IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items) =>
        items.Select(item => new TrackSector(item.Address.Number, item.Block.Data,
            SizeCode: SizeCode(item.Block.Data.Count), Attributes: TagAttributes(item.Block.Tag))).ToArray();

    public virtual IReadOnlyDictionary<string, int>? TrackAttributes(SectorImage image, int sectorCount) => null;
    public virtual uint BitCellTicks(SectorImage image, int cylinder) => 40;

    protected static IReadOnlyDictionary<string, int>? TagAttributes(IReadOnlyList<byte>? tag)
    {
        if (tag is null || tag.Count == 0) return null;
        return tag.Select((value, index) => (Key: $"tag{index}", Value: (int)value))
            .ToDictionary(item => item.Key, item => item.Value);
    }

    protected static byte? SizeCode(int size) => size switch
    {
        128 => 0, 256 => 1, 512 => 2, 1024 => 3,
        2048 => 4, 4096 => 5, 8192 => 6, 16384 => 7,
        _ => null
    };
}
