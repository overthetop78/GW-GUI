using GWGUI.MediaEngine.Images.Reading.Decoding;
using GWGUI.MediaEngine.Images.Formats.Floppy.Apple.Encoding.Definitions;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;

namespace GWGUI.MediaEngine.Images.Reading.Reconstruction.Sectors.Apple;

/// <summary>Construit une image sectorielle Apple II RWTS18 depuis des pistes déjà décodées.</summary>
internal static class AppleRwts18SectorImageBuilder
{
    /// <summary>Sélectionne le meilleur exemplaire de chaque secteur RWTS18 et construit leurs blocs logiques.</summary>
    public static SectorImage Create(IEnumerable<(int Track, IReadOnlyList<DecodedSector> Sectors)> decodedTracks)
    {
        var tracks = decodedTracks.ToArray();
        var selected = DecodedAppleSectorSelection.Best(tracks, sector => sector.Data is { Count: AppleRwts18Format.SectorByteCount } && sector.Number is >= 0 and < AppleRwts18Format.SectorCount);
        var blocks = selected.Select(item => new SectorBlock(item.Key.Track * AppleRwts18Format.SectorCount + item.Key.Sector, new(item.Key.Track, 0, item.Key.Sector), item.Value.Data!.ToArray(), item.Value.IntegrityValid)).ToArray();
        if (blocks.Length == 0) throw AppleSectorImageBuilderExceptions.NoRwts18Sector(tracks.Length, tracks.Sum(track => track.Sectors.Count));
        var trackCount = Math.Max(AppleIIGeometry.TrackCount, blocks.Max(block => block.Address.Cylinder) + 1);
        return new(DiskImageFormatIds.AppleIIRwts18, AppleRwts18Format.SectorByteCount, trackCount, DiskGeometryConstants.SingleSidedHeadCount, AppleRwts18Format.SectorCount, blocks);
    }
}
