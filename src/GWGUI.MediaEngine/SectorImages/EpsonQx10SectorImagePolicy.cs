using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.Reconstruction.EpsonQx10;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Détecte et construit les images sectorielles Epson QX-10.</summary>
internal static class EpsonQx10SectorImagePolicy
{
    /// <summary>Construit l'image selon l'identifiant de disposition demandé.</summary>
    public static SectorImage CreateImage(string formatId, IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates) => EpsonQx10SectorImageBuilder.Create(formatId, candidates);

    public static bool TryDetectFormat(IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates, out string formatId)
    {
        var sectors = candidates.Select(Describe).Where(descriptor => descriptor is not null).Select(descriptor => descriptor!.Value).ToArray();
        return EpsonQx10FormatDetector.TryDetect(sectors, out formatId);
    }

    /// <summary>Crée un descripteur seulement lorsqu'au moins un candidat contient des données.</summary>
    private static EpsonQx10SectorDescriptor? Describe(KeyValuePair<SectorAddress, List<IsoSectorCandidate>> pair)
    {
        var withData = pair.Value.Where(value => value.Sector.Data is not null).ToArray();
        if (withData.Length == 0) return null;
        var size = withData.GroupBy(value => value.Sector.IntegrityValid == true ? 2 : value.Sector.IntegrityValid is null ? 1 : 0).OrderByDescending(group => group.Key).First().GroupBy(value => value.Sector.Data!.Count).OrderByDescending(group => group.Count()).ThenByDescending(group => group.Key).First().Key;
        return new(pair.Key.Cylinder, pair.Key.Head, pair.Key.Number, size);
    }
}
