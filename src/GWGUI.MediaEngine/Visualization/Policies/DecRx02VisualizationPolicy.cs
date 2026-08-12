using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Visualization;

namespace GWGUI.MediaEngine.Visualization.Policies;

/// <summary>Détermine l'encodage et le découpage physique de visualisation des images DEC RX02.</summary>
internal sealed class DecRx02VisualizationPolicy : SectorImageVisualizationPolicy
{
    /// <inheritdoc />
    public override bool CanHandle(SectorImage image) => image.FormatId.Equals(DiskImageFormatIds.DecRx02, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string EncoderId(SectorImage image) => FluxCodecIds.DecRx02;

    /// <inheritdoc />
    public override IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image, IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items)
    {
        var sectors = new List<TrackSector>(items.Count * DecRx02Geometry.PhysicalSectorsPerLogicalBlock);
        foreach (var item in items)
        {
            if (item.Block.Data.Count < DecRx02Geometry.LogicalBlockSize) continue;
            var first = (item.Address.Number - DecRx02Geometry.FirstLogicalSectorNumber) * DecRx02Geometry.PhysicalSectorsPerLogicalBlock + DecRx02Geometry.FirstLogicalSectorNumber;
            sectors.Add(new(first, item.Block.Data.Take(DecRx02Geometry.PhysicalSectorSize).ToArray(), SizeCode: SectorSizeCode.FromByteCount(DecRx02Geometry.PhysicalSectorSize)));
            sectors.Add(new(first + 1, item.Block.Data.Skip(DecRx02Geometry.PhysicalSectorSize).Take(DecRx02Geometry.PhysicalSectorSize).ToArray(), SizeCode: SectorSizeCode.FromByteCount(DecRx02Geometry.PhysicalSectorSize)));
        }
        return sectors;
    }
}
