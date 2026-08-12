using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Regroupe les candidats ISO selon leur adresse décodée et selon leur piste physique source.</summary>
/// <param name="Addressed">Candidats dont l'adresse décodée correspond à la piste physique.</param>
/// <param name="Physical">Candidats conservés à l'adresse de leur piste physique, même lorsque l'en-tête diffère.</param>
internal sealed record IsoSectorCandidateSet(
    IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> Addressed,
    IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> Physical);
