namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding.Sectors;

/// <summary>Associe l'identifiant d'un candidat SCP à l'exception précise de son rejet.</summary>
internal sealed record ScpCandidateFailure(string CandidateId, Exception Exception);
