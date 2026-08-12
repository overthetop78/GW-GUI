namespace GWGUI.MediaEngine.Reconstruction;

/// <summary>Sélectionne le meilleur candidat selon une priorité d'intégrité commune.</summary>
internal static class SectorCandidateSelector
{
    /// <summary>Privilégie une intégrité valide, puis inconnue, puis invalide.</summary>
    /// <typeparam name="T">Type du candidat sélectionné.</typeparam>
    /// <param name="candidates">Candidats à classer.</param>
    /// <param name="integrity">Fonction retournant l'intégrité du candidat.</param>
    /// <returns>Le candidat possédant la meilleure intégrité.</returns>
    /// <exception cref="InvalidOperationException">La collection ne contient aucun candidat.</exception>
    public static T Best<T>(IEnumerable<T> candidates, Func<T, bool?> integrity) => candidates.OrderByDescending(candidate => integrity(candidate) == true).ThenByDescending(candidate => integrity(candidate) is null).First();
}
