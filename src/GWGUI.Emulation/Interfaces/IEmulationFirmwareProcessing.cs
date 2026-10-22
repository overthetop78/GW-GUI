namespace GWGUI.Emulation.Interfaces;

/// <summary>Parcours communs des fichiers de firmware, indépendants du catalogue du module.</summary>
/// <remarks>Le module conserve l’identification, la compatibilité et la destination des fichiers. Le service emprunte les inspecteurs pendant le parcours ; il ne conserve aucun candidat ni handle après son retour.</remarks>
public interface IEmulationFirmwareProcessing
{
    /// <summary>Crée le dossier si nécessaire et inspecte séquentiellement ses fichiers par chemin ordinal insensible à la casse.</summary>
    /// <param name="directory">Racine du parcours plat, sans recherche dans les sous-dossiers.</param>
    /// <param name="inspect">Inspecteur du module ; son résultat est ajouté sans modifier son identité ni masquer ses erreurs.</param>
    /// <param name="cancellationToken">Annulation vérifiée avant chaque fichier et transmise à l’inspecteur.</param>
    /// <returns>Candidats dans l’ordre des fichiers inspectés.</returns>
    ValueTask<IReadOnlyList<TCandidate>> ScanDirectoryAsync<TCandidate>(string directory, Func<string, CancellationToken, ValueTask<TCandidate>> inspect, CancellationToken cancellationToken);

    /// <summary>Parcourt récursivement les fichiers triés en excluant les points de réanalyse.</summary>
    /// <param name="directory">Racine créée si elle est absente.</param>
    /// <param name="inspect">Action du module pouvant ajouter plusieurs candidats à sa propre collection.</param>
    /// <param name="cancellationToken">Annulation vérifiée avant chaque inspection ; les erreurs de l’inspecteur sont propagées.</param>
    void ScanRecursive(string directory, Action<string> inspect, CancellationToken cancellationToken);
}
