namespace GWGUI.Scp.SectorImages;

internal sealed record IsoSectorCandidateSet(
    IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> Addressed,
    IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> Physical);
