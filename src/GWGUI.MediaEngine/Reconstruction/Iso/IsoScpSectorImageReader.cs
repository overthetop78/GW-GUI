using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Reconstruit une image sectorielle depuis des secteurs ISO FM ou MFM décodés d'une capture SCP.</summary>
/// <param name="scpReader">Lecteur utilisé pour analyser le conteneur SCP.</param>
/// <param name="decoders">Registre fournissant les décodeurs ISO FM et MFM.</param>
public sealed class IsoScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly IsoScpCandidateDecoder candidateDecoder = new(scpReader, decoders);

    /// <summary>Décode la capture une fois puis applique la politique de reconstruction demandée.</summary>
    /// <param name="path">Chemin de la capture SCP à reconstruire.</param>
    /// <param name="formatId">Identifiant demandé, ou <see langword="null"/> pour une sélection automatique.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et le décodage.</param>
    /// <returns>L'image sectorielle construite par la politique ISO résolue.</returns>
    public async Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        var policy = IsoScpSectorImagePolicyRegistry.Resolve(formatId);
        var candidates = await candidateDecoder.DecodeAsync(path, policy.DecoderIds, cancellationToken).ConfigureAwait(false);
        if (candidates.Addressed.Count == 0 && candidates.Physical.Count == 0)
            throw IsoScpReconstructionExceptions.NoCandidates(formatId, candidates.Addressed.Count, candidates.Physical.Count);
        return policy.Build(formatId, candidates);
    }
}
