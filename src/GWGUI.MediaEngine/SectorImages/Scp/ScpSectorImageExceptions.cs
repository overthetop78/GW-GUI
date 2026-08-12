namespace GWGUI.MediaEngine.SectorImages.Scp;

/// <summary>Construit les erreurs détaillées de reconstruction sectorielle SCP.</summary>
internal static class ScpSectorImageExceptions
{
    /// <summary>Crée une erreur totale en conservant le chemin, le format demandé et chaque rejet.</summary>
    public static AggregateException AllCandidatesRejected(string path, string? formatId, IReadOnlyList<ScpCandidateFailure> failures) => new($"Aucun candidat SCP n'a décodé '{path}' pour le format '{formatId ?? "automatique"}'.", failures.Select(failure => new InvalidDataException($"Candidat '{failure.CandidateId}' rejeté.", failure.Exception)));
}
