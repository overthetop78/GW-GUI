using GWGUI.MediaEngine.Decoding;

namespace GWGUI.MediaEngine.SectorImages.Builders.Apple;

/// <summary>Sélectionne le meilleur exemplaire de secteurs Apple décodés plusieurs fois.</summary>
internal static class DecodedAppleSectorSelection
{
    /// <summary>Conserve un secteur par piste et numéro en donnant la priorité à une intégrité explicitement valide.</summary>
    public static IReadOnlyDictionary<(int Track, int Sector), DecodedSector> Best(IEnumerable<(int Track, IReadOnlyList<DecodedSector> Sectors)> decodedTracks, Func<DecodedSector, bool> accept) => decodedTracks.SelectMany(item => item.Sectors.Where(accept).Select(sector => (item.Track, Sector: sector))).GroupBy(item => (item.Track, item.Sector.Number)).ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.Sector.IntegrityValid == true).First().Sector);
}
