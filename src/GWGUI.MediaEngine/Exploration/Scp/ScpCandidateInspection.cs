using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Scp;

/// <summary>Conserve l'identité, l'image, les correspondances normalisées et le diagnostic d'un candidat SCP.</summary>
internal sealed record ScpCandidateInspection(
    string CandidateId,
    SectorImage? Image,
    IReadOnlyList<ExploredFileSystem> Matches,
    InvalidDataException? Diagnostic);
