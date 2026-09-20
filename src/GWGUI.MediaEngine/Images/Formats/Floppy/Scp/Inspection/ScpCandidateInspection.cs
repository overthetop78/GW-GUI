using GWGUI.MediaEngine.Exploration.Results;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;

/// <summary>Conserve l'identité, l'image, les correspondances normalisées et le diagnostic d'un candidat SCP.</summary>
internal sealed record ScpCandidateInspection(
    string CandidateId,
    SectorImage? Image,
    IReadOnlyList<ExploredFileSystem> Matches,
    InvalidDataException? Diagnostic);
