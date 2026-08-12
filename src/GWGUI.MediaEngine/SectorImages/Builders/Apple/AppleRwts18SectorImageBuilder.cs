using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.SectorImages.Builders.Apple;

/// <summary>Construit une image sectorielle Apple II RWTS18 depuis des pistes déjà décodées.</summary>
internal static class AppleRwts18SectorImageBuilder
{
    /// <summary>Sélectionne le meilleur exemplaire de chaque secteur RWTS18 et construit leurs blocs logiques.</summary>
    public static SectorImage Create(IEnumerable<(int Track, IReadOnlyList<DecodedSector> Sectors)> decodedTracks)
    {
        var selected = DecodedAppleSectorSelection.Best(decodedTracks, sector => sector.Data is { Count: AppleRwts18Format.SectorByteCount } && sector.Number is >= 0 and < AppleRwts18Format.SectorCount);
        var blocks = selected.Select(item => new SectorBlock(item.Key.Track * AppleRwts18Format.SectorCount + item.Key.Sector, new(item.Key.Track, 0, item.Key.Sector), item.Value.Data!.ToArray(), item.Value.IntegrityValid)).ToArray();
        if (blocks.Length == 0) throw new InvalidDataException("No Apple II RWTS18 sectors could be decoded.");
        var trackCount = Math.Max(AppleIIGeometry.TrackCount, blocks.Max(block => block.Address.Cylinder) + 1);
        return new(DiskImageFormatIds.AppleIIRwts18, AppleRwts18Format.SectorByteCount, trackCount, DiskGeometryConstants.SingleSidedHeadCount, AppleRwts18Format.SectorCount, blocks);
    }
}
