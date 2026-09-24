namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding.Sectors;

/// <summary>Associe un prédicat de format explicite au candidat SCP à exécuter.</summary>
internal sealed record ScpFormatSelection(Predicate<string> Matches, ScpSectorImageCandidate Candidate);
