namespace GWGUI.Emulation.Sinclair.Common.Interfaces;

/// <summary>Inspection en lecture seule d’un fichier candidat par les règles de l’adaptateur de cœur.</summary>
/// <remarks>Common fournit la configuration courante et transmet le résultat au gestionnaire de firmwares.
/// Cette opération reste distincte de la préparation des fichiers de session et de la description des emplacements.
/// L’adaptateur réutilise son identification existante et laisse le fichier d’origine sous la responsabilité de son appelant.</remarks>
internal interface IEmulatorFirmwareAdapter
{
    /// <summary>Recherche les candidats dans le dossier de firmware de la configuration.</summary>
    /// <param name="configuration">Configuration empruntée déterminant les rôles et compatibilités attendus.</param>
    /// <param name="directory">Dossier de recherche fourni par Common ; les fichiers sources restent empruntés.</param>
    /// <param name="cancellationToken">Annulation du parcours et des lectures nécessaires à la reconnaissance.</param>
    /// <returns>Candidats dans l'ordre du parcours natif ; plusieurs rôles d'un même fichier restent distincts.</returns>
    /// <remarks>Le parcours de base inspecte les fichiers du dossier dans l'ordre ordinal insensible à la casse
    /// avec le service de firmware Common. Le backend conserve son parcours récursif ou son catalogue
    /// lorsqu'il en possède un. L'inspection par fichier reste portée par ScanFirmwareFileAsync ; aucune
    /// recherche n'installe de firmware ni ne conserve de flux ouvert après son retour.</remarks>
    ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(MachineConfiguration configuration, string directory, CancellationToken cancellationToken);

    /// <summary>Décrit les emplacements de firmware de la configuration complète.</summary>
    /// <param name="configuration">Modèle, cœur et options déterminant les champs et leurs contraintes.</param>
    /// <returns>Emplacements dans l'ordre du catalogue natif, avec leurs métadonnées disponibles.</returns>
    /// <remarks>Les descriptions sont empruntées. Sans catalogue propre au backend, Common reprend les champs
    /// de fichiers de firmware déjà décrits dans ses réglages, avec les mêmes identifiants et clés d'aide.
    /// Les noms de destination et obligations non déclarés restent absents ; les contrôles natifs restent applicables.</remarks>
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);

    /// <summary>Analyse un fichier suivant le modèle, le cœur et les options de la configuration.</summary>
    /// <param name="configuration">Configuration empruntée de la machine qui utilisera le firmware.</param>
    /// <param name="path">Chemin du fichier candidat à lire.</param>
    /// <param name="cancellationToken">Annulation des lectures et calculs d’empreintes.</param>
    /// <returns>Candidats dans leur ordre de reconnaissance, avec leur identité, compatibilité et destination. Une liste vide signifie que le fichier ne fournit aucun candidat ; plusieurs résultats conservent les rôles et variantes possibles sans en choisir un arbitrairement.</returns>
    /// <remarks>Les flux ouverts pour l’analyse sont libérés avant le retour ; le fichier n’est ni installé ni modifié.</remarks>
    ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken);

    /// <summary>Prépare les fichiers de firmware nécessaires au dossier système de la session.</summary>
    /// <param name="configuration">Configuration empruntée contenant les firmwares choisis et les options du cœur.</param>
    /// <param name="systemDirectory">Dossier système fourni par le propriétaire de la session.</param>
    /// <remarks>Les adaptations conservent leurs vérifications et leurs noms de destination. Les fichiers
    /// sources restent empruntés ; le propriétaire de la session conserve le nettoyage des copies préparées.
    /// Le comportement Common crée le dossier lorsque le cœur ne possède aucun traitement spécifique.
    /// La synchronisation des fichiers persistants conserve son propre cycle de vie et son propriétaire.</remarks>
    void PrepareFirmware(MachineConfiguration configuration, string systemDirectory);
}
