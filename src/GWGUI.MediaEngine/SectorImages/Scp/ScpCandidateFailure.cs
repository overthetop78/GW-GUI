namespace GWGUI.MediaEngine.SectorImages.Scp;

/// <summary>Associe l'identifiant d'un candidat SCP à l'exception précise de son rejet.</summary>
internal sealed record ScpCandidateFailure(string CandidateId, Exception Exception);
