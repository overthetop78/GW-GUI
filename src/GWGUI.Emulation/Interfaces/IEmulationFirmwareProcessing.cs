namespace GWGUI.Emulation.Interfaces;

/// <summary>Parcours communs des fichiers de firmware, indépendants du catalogue du module.</summary>
/// <remarks>Le module conserve l’identification, la compatibilité et la destination des fichiers. Le service emprunte les inspecteurs pendant le parcours ; il ne conserve aucun candidat ni handle après son retour.</remarks>
public interface IEmulationFirmwareProcessing
{
    /// <summary>Calcule le CRC32 standard utilisé pour identifier les données d'un firmware.</summary>
    /// <param name="data">Octets empruntés, consultés sans copie, modification ni accès à un fichier.</param>
    /// <returns>CRC32 numérique non signé ; zéro pour une étendue vide.</returns>
    /// <remarks>Le calcul utilise le polynôme réfléchi 0xEDB88320, une valeur initiale de 0xFFFFFFFF
    /// et un complément final. Il retourne la valeur numérique ; le module conserve son choix de formatage
    /// et ses règles d'identification. Ce CRC ne constitue pas une empreinte cryptographique.</remarks>
    uint ComputeCrc32(ReadOnlySpan<byte> data);

    /// <summary>Retrouve un candidat unique sans relire son fichier ni modifier les résultats de reconnaissance.</summary>
    /// <param name="candidates">Résultats empruntés d’une même inspection, conservés dans leur ordre.</param>
    /// <param name="candidateId">Identité sélectionnée, ou null si seul le rôle est connu.</param>
    /// <param name="destinationFieldId">Rôle demandé, ou null si aucune destination n’est précisée.</param>
    /// <returns>Le candidat de même identité et destination, puis de même rôle ; à défaut, le seul candidat de la liste.</returns>
    /// <exception cref="InvalidOperationException">Aucun résultat ou plusieurs résultats ne permettent une sélection unique.</exception>
    /// <remarks>La sélection par rôle conserve les alternatives d’un même fichier. Une inspection à un seul résultat
    /// conserve son résultat, y compris son diagnostic d’incompatibilité, pour que Common applique ses contrôles habituels.</remarks>
    EmulationFirmwareCandidate SelectCandidate(IReadOnlyList<EmulationFirmwareCandidate> candidates, string? candidateId, string? destinationFieldId);

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
