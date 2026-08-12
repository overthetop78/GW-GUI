using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Reconstruction;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Mesure des candidats ISO et construit des images sectorielles uniformes.</summary>
internal static class IsoSectorImageBuilder
{
    /// <summary>Mesure la géométrie majoritaire des candidats adressés.</summary>
    /// <param name="candidates">Candidats ISO regroupés par adresse physique.</param>
    /// <returns>La taille sectorielle, la géométrie et la numérotation majoritaires observées.</returns>
    /// <exception cref="InvalidDataException">Aucun candidat adressé n'est disponible.</exception>
    public static IsoSectorMeasurement Measure(IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates)
    {
        if (candidates.Count == 0) throw IsoScpReconstructionExceptions.NoAddressedCandidates();

        var sectorSize = candidates.Values.SelectMany(value => value)
            .GroupBy(value => value.Sector.Data!.Count).OrderByDescending(group => group.Count()).First().Key;
        var cylinders = candidates.Keys.Max(address => address.Cylinder) + 1;
        var heads = candidates.Keys.Max(address => address.Head) + 1;
        var sectorsPerTrack = candidates.Keys.GroupBy(address => (address.Cylinder, address.Head))
            .Select(group => group.Select(item => item.Number).Distinct().Count())
            .GroupBy(count => count).OrderByDescending(group => group.Count()).ThenByDescending(group => group.Key).First().Key;
        var sectorOrder = candidates.Keys.Select(address => address.Number).Distinct().OrderBy(number => number).ToArray();
        return new(sectorSize, cylinders, heads, sectorsPerTrack, sectorOrder, sectorOrder.Length > 0 && sectorOrder[0] == 0);
    }

    /// <summary>Construit une image uniforme en filtrant les adresses incompatibles avec la géométrie demandée.</summary>
    /// <param name="formatId">Identifiant de l'image construite.</param>
    /// <param name="candidates">Candidats regroupés par adresse.</param>
    /// <param name="sectorSize">Taille sectorielle nominale, en octets.</param>
    /// <param name="cylinders">Nombre de cylindres.</param>
    /// <param name="heads">Nombre de faces.</param>
    /// <param name="sectorsPerTrack">Nombre de secteurs par piste.</param>
    /// <param name="sectorIndex">Fonction convertissant une adresse en index sectoriel à base zéro.</param>
    /// <param name="allowSectorNumbersBeyondGeometry">Autorise les numéros physiques dépassant le nombre logique de secteurs.</param>
    /// <param name="allowVariableBlockSize">Autorise des données dont la taille diffère de la taille nominale.</param>
    /// <param name="capacity">Capacité explicite en octets, ou <see langword="null"/> pour la capacité géométrique.</param>
    /// <returns>L'image uniforme contenant les meilleurs candidats utilisables.</returns>
    public static SectorImage CreateUniform(
        string formatId,
        IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates,
        int sectorSize,
        int cylinders,
        int heads,
        int sectorsPerTrack,
        Func<SectorAddress, int> sectorIndex,
        bool allowSectorNumbersBeyondGeometry = false,
        bool allowVariableBlockSize = false,
        long? capacity = null)
    {
        var blocks = new List<SectorBlock>();
        foreach (var (address, values) in candidates)
        {
            if (!allowSectorNumbersBeyondGeometry && address.Number > sectorsPerTrack) continue;
            if (address.Cylinder >= cylinders || address.Head >= heads) continue;
            var index = sectorIndex(address);
            if (index < 0 || index >= sectorsPerTrack) continue;
            var best = Best(values);
            var logical = (address.Cylinder * heads + address.Head) * sectorsPerTrack + index;
            blocks.Add(new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution));
        }
        return new(formatId, sectorSize, cylinders, heads, sectorsPerTrack, blocks, allowVariableBlockSize: allowVariableBlockSize, capacity: capacity);
    }

    /// <summary>Retourne les données non vides du meilleur candidat disponible à une adresse.</summary>
    /// <param name="candidates">Candidats ISO regroupés par adresse physique.</param>
    /// <param name="address">Adresse physique recherchée.</param>
    /// <returns>Une copie des meilleures données, ou un tableau vide si l'adresse ou ses données sont absentes.</returns>
    public static byte[] BestData(IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates, SectorAddress address)
    {
        if (!candidates.TryGetValue(address, out var values)) return [];
        var withData = values.Where(value => value.Sector.Data is { Count: > 0 }).ToArray();
        return withData.Length == 0 ? [] : Best(withData).Sector.Data!.ToArray();
    }

    /// <summary>Retourne le candidat prioritaire selon la règle d'intégrité commune.</summary>
    /// <param name="candidates">Candidats à classer.</param>
    /// <returns>Le candidat dont l'intégrité est valide, sinon inconnue, sinon invalide.</returns>
    /// <exception cref="InvalidOperationException">La collection ne contient aucun candidat.</exception>
    private static IsoSectorCandidate Best(IEnumerable<IsoSectorCandidate> candidates) => SectorCandidateSelector.Best(candidates, candidate => candidate.Sector.IntegrityValid);
}
