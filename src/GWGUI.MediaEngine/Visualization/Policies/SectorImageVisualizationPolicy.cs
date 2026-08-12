using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Visualization;

namespace GWGUI.MediaEngine.Visualization.Policies;

/// <summary>Fournit les comportements communs des politiques de visualisation sectorielle.</summary>
internal abstract class SectorImageVisualizationPolicy : ISectorImageVisualizationPolicy
{
    /// <inheritdoc />
    public abstract bool CanHandle(SectorImage image);
    /// <inheritdoc />
    public abstract string EncoderId(SectorImage image);
    /// <inheritdoc />
    public virtual SectorAddress VisualAddress(SectorImage image, SectorAddress address) => address;

    /// <inheritdoc />
    public virtual IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image, IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items) => items.Select(item => new TrackSector(item.Address.Number, item.Block.Data, SizeCode: SectorSizeCode.TryFromByteCount(item.Block.Data.Count, out var code) ? code : null, Attributes: TagAttributes(item.Block.Tag))).ToArray();

    /// <inheritdoc />
    public virtual IReadOnlyDictionary<string, int>? TrackAttributes(SectorImage image, int sectorCount) => null;
    /// <inheritdoc />
    public virtual uint BitCellTicks(SectorImage image, int cylinder) => TrackEncodingDefaults.BitCellTicks;

    /// <summary>Convertit les tags binaires d'un secteur en attributs indexés d'encodage.</summary>
    /// <param name="tag">Tags du secteur, ou <see langword="null"/>.</param>
    /// <returns>Attributs indexés, ou <see langword="null"/> en l'absence de tags.</returns>
    protected static IReadOnlyDictionary<string, int>? TagAttributes(IReadOnlyList<byte>? tag)
    {
        if (tag is null || tag.Count == 0) return null;
        return tag.Select((value, index) => (Key: TrackEncodingAttributeKeys.Tag(index), Value: (int)value)).ToDictionary(item => item.Key, item => item.Value);
    }
}
