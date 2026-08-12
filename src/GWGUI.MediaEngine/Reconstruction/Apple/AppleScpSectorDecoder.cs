using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Apple;

/// <summary>Décode et sélectionne les candidats sectoriels communs aux reconstructeurs Apple SCP.</summary>
/// <param name="decoders">Registre fournissant les décodeurs de flux Apple demandés.</param>
internal sealed class AppleScpSectorDecoder(FluxDecoderRegistry decoders)
{
    private readonly AppleMacGcrDecoder _macDecoder = new();

    /// <summary>Décode toutes les révolutions avec le codec demandé et regroupe les candidats par adresse.</summary>
    /// <param name="scp">Capture SCP contenant les pistes et révolutions à décoder.</param>
    /// <param name="decoderId">Identifiant technique du décodeur de flux.</param>
    /// <param name="size">Taille attendue de chaque secteur, en octets.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le parcours des pistes.</param>
    /// <returns>Les candidats de taille valide, regroupés par adresse avec leur numéro de révolution à base un.</returns>
    public Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> DecodeCandidates(ScpImage scp, string decoderId, int size, CancellationToken cancellationToken)
    {
        var result = new Dictionary<SectorAddress, List<(DecodedSector, int)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            {
                var decoded = decoderId == FluxCodecIds.AppleMacGcr
                    ? DecodeMacTrack(track, track.Revolutions[revolution])
                    : decoders.Decode(decoderId, track.Revolutions[revolution].Flux);
                foreach (var sector in decoded.Sectors)
                {
                    if (sector.Data is not { Count: var length } || length != size) continue;
                    var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                    if (!result.TryGetValue(address, out var list)) result[address] = list = [];
                    list.Add((sector, revolution + 1));
                }
            }
        }
        return result;
    }

    /// <summary>Sélectionne le meilleur candidat d'une adresse selon son intégrité.</summary>
    /// <param name="logical">Numéro de bloc logique à attribuer au secteur sélectionné.</param>
    /// <param name="address">Adresse physique commune aux candidats.</param>
    /// <param name="values">Candidats et numéros de révolution disponibles pour cette adresse.</param>
    /// <returns>Le bloc construit avec le candidat dont l'intégrité est la meilleure.</returns>
    public static SectorBlock Select(int logical, SectorAddress address, List<(DecodedSector Sector, int Revolution)> values)
    {
        var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true)
            .ThenByDescending(value => value.Sector.IntegrityValid is null).First();
        return new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution, best.Sector.Tag?.ToArray(), best.Sector.FormatCode);
    }

    /// <summary>Décode une révolution Macintosh avec plusieurs durées de cellule voisines.</summary>
    /// <param name="track">Piste SCP dont le cylindre et la face bornent les secteurs plausibles.</param>
    /// <param name="revolution">Révolution contenant le flux à essayer avec plusieurs durées de cellule.</param>
    /// <returns>Le résultat de décodage obtenant le meilleur score de secteurs Macintosh plausibles.</returns>
    private FluxDecodeResult DecodeMacTrack(ScpTrack track, ScpRevolution revolution)
    {
        var expected = MacintoshGcrGeometry.Sectors(track.Cylinder);
        var initial = FluxTimingEstimator.EstimateNonFmBitCell(revolution.FluxIntervals) * 2;
        var factors = AppleScpReconstructionDefinitions.MacintoshBitCellFactors;
        FluxDecodeResult? best = null;
        var bestScore = int.MinValue;
        foreach (var factor in factors)
        {
            var candidate = _macDecoder.DecodeAtBitCell(revolution.Flux, initial * factor);
            var plausible = candidate.Sectors.Where(sector => sector.Data?.Count == AppleIwmGcrFormat.SectorByteCount &&
                sector.Cylinder == track.Cylinder && sector.Head == track.Head &&
                sector.Number >= 0 && sector.Number < expected).ToArray() ?? [];
            var score = plausible.Select(sector => sector.Number).Distinct().Count() * AppleScpReconstructionDefinitions.DistinctSectorScoreWeight + plausible.Count(sector => sector.IntegrityValid == true) * AppleScpReconstructionDefinitions.ValidSectorScoreWeight + plausible.Length;
            if (score > bestScore)
            {
                best = candidate;
                bestScore = score;
            }
            if (plausible.Where(sector => sector.IntegrityValid == true).Select(sector => sector.Number)
                    .Distinct().Count() == expected) break;
        }
        return best ?? _macDecoder.Decode(revolution.Flux);
    }
}
