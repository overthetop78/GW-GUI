using GWGUI.MediaEngine.Images.Writing.Encoding;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Visualization;
using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Images.Formats.Floppy.Rx02;


namespace GWGUI.MediaEngine.Images.Visualization.Policies;

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
