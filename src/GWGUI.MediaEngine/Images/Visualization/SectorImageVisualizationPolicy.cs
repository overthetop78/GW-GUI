using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Visualization;

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

    protected static byte? SizeCode(int size) => SectorSizeCode.TryFromByteCount(size, out var code) ? code : null;
}
