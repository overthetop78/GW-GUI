using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Reconstruit une image sectorielle depuis des secteurs ISO FM ou MFM décodés d'une capture SCP.</summary>
/// <param name="scpReader">Lecteur utilisé pour analyser le conteneur SCP.</param>
/// <param name="decoders">Registre fournissant les décodeurs ISO FM et MFM.</param>
public sealed class IsoScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    /// <summary>Lit la capture, sélectionne le meilleur décodeur par révolution et applique la politique demandée.</summary>
    /// <param name="path">Chemin de la capture SCP à reconstruire.</param>
    /// <param name="formatId">Identifiant demandé, ou <see langword="null"/> pour une sélection automatique.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et le décodage.</param>
    /// <returns>L'image sectorielle construite par la politique ISO résolue.</returns>
    /// <exception cref="InvalidDataException">Aucun candidat adressé ni physique ne peut être décodé, ou la politique résolue rejette les candidats.</exception>
    public async Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var policy = IsoScpSectorImagePolicyRegistry.Resolve(formatId);
        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        var physicalCandidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            {
                var result = policy.DecoderIds.Select(decoder => decoders.Decode(decoder, track.Revolutions[revolution].Flux))
                    .OrderByDescending(Score).First();
                foreach (var sector in result.Sectors)
                {
                    if (sector.Data is null || sector.Number < 0) continue;
                    AddCandidate(physicalCandidates, new(track.Cylinder, track.Head, sector.Number), sector, revolution + 1);
                    if (sector.Cylinder != track.Cylinder || sector.Head != track.Head) continue;
                    var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                    AddCandidate(candidates, address, sector, revolution + 1);
                }
            }
        }
        if (candidates.Count == 0 && physicalCandidates.Count == 0) throw IsoScpReconstructionExceptions.NoCandidates(formatId, candidates.Count, physicalCandidates.Count);
        return policy.Build(formatId, new(candidates, physicalCandidates));
    }

    /// <summary>Calcule le score d'un résultat de décodage ISO.</summary>
    /// <param name="result">Résultat de décodage à classer.</param>
    /// <returns>La confiance augmentée du poids de chaque secteur contenant des données.</returns>
    private static double Score(FluxDecodeResult result) => result.Sectors.Count(sector => sector.Data is not null) * IsoScpReconstructionDefinitions.DataSectorScoreWeight + result.Confidence;

    /// <summary>Ajoute un candidat à la collection de son adresse physique ou logique.</summary>
    /// <param name="candidates">Collection regroupant les candidats par adresse.</param>
    /// <param name="address">Adresse à laquelle rattacher le secteur.</param>
    /// <param name="sector">Secteur décodé.</param>
    /// <param name="revolution">Numéro de révolution à base un.</param>
    private static void AddCandidate(Dictionary<SectorAddress, List<IsoSectorCandidate>> candidates, SectorAddress address, DecodedSector sector, int revolution)
    {
        if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
        list.Add(new(sector, revolution));
    }
}
