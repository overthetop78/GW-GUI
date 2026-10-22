namespace GWGUI.Emulation.Interfaces;

/// <summary>Construction des ressources natives communes, sans exposition de leurs classes d’implémentation.</summary>
/// <remarks>Le service est emprunté et sans ressource de session. Chaque résultat possède ses propres
/// allocations ; l’appelant les libère dans un finally, sauf transfert réussi à un autre propriétaire.</remarks>
public interface IEmulationNativeResources
{
    /// <summary>Copie un texte en UTF-8 terminé par zéro dans une allocation native indépendante.</summary>
    /// <param name="value">Texte à copier ; null est refusé.</param>
    /// <param name="minimumByteCapacity">Capacité minimale en octets, terminateur compris ; le complément est nul.</param>
    /// <returns>Chaîne possédée par l’appelant ; sa capacité couvre toujours le texte et son terminateur.</returns>
    IEmulationNativeString CreateString(string value, int minimumByteCapacity = 0);

    /// <summary>Copie un texte en ANSI sous Windows si demandé, en UTF-8 dans les autres cas.</summary>
    /// <param name="value">Texte à copier ; null est refusé.</param>
    /// <param name="useWindowsAnsi">Sélection de l’encodage ANSI Windows pour les cœurs qui l’exigent.</param>
    /// <returns>Chaîne possédée par l’appelant, libérée après son dernier emprunt natif.</returns>
    IEmulationNativeString CreateString(string value, bool useWindowsAnsi);

    /// <summary>Construit une chaîne UTF-8 et en transfère la propriété au dictionnaire si l’ajout réussit.</summary>
    /// <param name="owner">Propriétaire qui libérera les chaînes enregistrées lors de son nettoyage.</param>
    /// <param name="key">Clé transmise à Add ; les règles et erreurs du dictionnaire sont conservées.</param>
    /// <param name="value">Texte à copier dans la nouvelle allocation.</param>
    /// <returns>Emprunt de la chaîne désormais possédée par le dictionnaire.</returns>
    /// <remarks>Si l’ajout échoue, la nouvelle allocation est libérée et l’exception d’origine est propagée.</remarks>
    IEmulationNativeString AddString(IDictionary<string, IEmulationNativeString> owner, string key, string value);

    /// <summary>Prépare le descripteur natif d’un fichier pour le contrat de chargement du cœur.</summary>
    /// <param name="absolutePath">Chemin déjà validé par le module, copié dans le descripteur.</param>
    /// <param name="needsFullPath">True pour transmettre seulement le chemin ; false pour copier aussi les octets du fichier.</param>
    /// <param name="useWindowsAnsiPath">Encodage ANSI du chemin sous Windows si le cœur l’exige.</param>
    /// <returns>Contenu possédé par l’appelant, qui libère ensemble descripteur, chemin et données.</returns>
    /// <remarks>La construction libère toutes ses allocations si une étape échoue. Elle ne modifie aucun fichier.</remarks>
    IEmulationLoadedContent CreateLoadedContent(string absolutePath, bool needsFullPath, bool useWindowsAnsiPath);
}
