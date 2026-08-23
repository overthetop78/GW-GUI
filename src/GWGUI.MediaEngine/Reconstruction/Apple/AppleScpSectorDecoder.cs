using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Reconstruction.Scp;

namespace GWGUI.MediaEngine.Reconstruction.Apple;

/// <summary>DÃ©code et sÃ©lectionne les candidats sectoriels communs aux reconstructeurs Apple SCP.</summary>
/// <param name="decoders">Registre fournissant les dÃ©codeurs de flux Apple demandÃ©s.</param>
internal sealed class AppleScpSectorDecoder(FluxDecoderRegistry decoders)
{
    private readonly AppleMacGcrDecoder _macDecoder = new();

    /// <summary>DÃ©code toutes les rÃ©volutions avec le codec demandÃ© et regroupe les candidats par adresse.</summary>
    /// <param name="scp">Capture SCP contenant les pistes et rÃ©volutions Ã  dÃ©coder.</param>
    /// <param name="decoderId">Identifiant technique du dÃ©codeur de flux.</param>
    /// <param name="size">Taille attendue de chaque secteur, en octets.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le parcours des pistes.</param>
    /// <returns>Les candidats de taille valide, regroupÃ©s par adresse avec leur numÃ©ro de rÃ©volution Ã  base un.</returns>
    public Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> DecodeCandidates(ScpImage scp, string decoderId, int size, CancellationToken cancellationToken)
    {
        var result = new Dictionary<SectorAddress, List<(DecodedSector, int)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (decoderId == FluxCodecIds.AppleMacGcr && track.Cylinder is < 0 or >= MacintoshGcrGeometry.CylinderCount) continue;
            foreach (var window in ScpTrackDecodeWindowFactory.Create(track))
            {
                var decoded = decoderId == FluxCodecIds.AppleMacGcr
                    ? DecodeMacTrack(track, window.Flux)
                    : decoders.Decode(decoderId, window.Flux);
                foreach (var sector in decoded.Sectors)
                {
                    if (sector.Data is not { Count: var length } || length != size) continue;
                    var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                    if (!result.TryGetValue(address, out var list)) result[address] = list = [];
                    list.Add((sector, window.Revolution));
                }
            }
        }
        return result;
    }

    /// <summary>SÃ©lectionne le meilleur candidat d'une adresse selon son intÃ©gritÃ©.</summary>
    /// <param name="logical">NumÃ©ro de bloc logique Ã  attribuer au secteur sÃ©lectionnÃ©.</param>
    /// <param name="address">Adresse physique commune aux candidats.</param>
    /// <param name="values">Candidats et numÃ©ros de rÃ©volution disponibles pour cette adresse.</param>
    /// <returns>Le bloc construit avec le candidat dont l'intÃ©gritÃ© est la meilleure.</returns>
    public static SectorBlock Select(int logical, SectorAddress address, List<(DecodedSector Sector, int Revolution)> values)
    {
        var best = SectorCandidateSelector.Best(values, value => value.Sector.IntegrityValid);
        return new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution, best.Sector.Tag?.ToArray(), best.Sector.FormatCode);
    }

    /// <summary>DÃ©code une rÃ©volution Macintosh avec plusieurs durÃ©es de cellule voisines.</summary>
    /// <param name="track">Piste SCP dont le cylindre et la face bornent les secteurs plausibles.</param>
    /// <param name="revolution">RÃ©volution contenant le flux Ã  essayer avec plusieurs durÃ©es de cellule.</param>
    /// <returns>Le rÃ©sultat de dÃ©codage obtenant le meilleur score de secteurs Macintosh plausibles.</returns>
    private FluxDecodeResult DecodeMacTrack(ScpTrack track, FluxRevolution revolution)
    {
        var expected = MacintoshGcrGeometry.Sectors(track.Cylinder);
        var initial = FluxTimingEstimator.EstimateNonFmBitCell(revolution.FluxIntervals) * 2;
        var factors = AppleScpReconstructionDefinitions.MacintoshBitCellFactors;
        FluxDecodeResult? best = null;
        var bestScore = int.MinValue;
        foreach (var factor in factors)
        {
            var candidate = _macDecoder.DecodeAtBitCell(revolution, initial * factor);
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
        return best ?? _macDecoder.Decode(revolution);
    }
}
