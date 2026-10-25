namespace GWGUI.Emulation.Coleco.Common.Interfaces;

/// <summary>Description des lecteurs et préparation des médias d'une configuration pour son adaptateur.</summary>
/// <remarks>Common utilise ces descriptions pour présenter et contrôler les montages des médias.
/// Les identités des emplacements, les types, les extensions et les capacités restent ceux du backend.
/// Ce contrat ne possède ni lecteur physique, ni fichier de contenu, ni session de cœur.</remarks>
internal interface IEmulatorMediaCapabilities
{
    /// <summary>Décrit les lecteurs disponibles pour le modèle et les options de la configuration.</summary>
    /// <param name="configuration">Configuration empruntée à consulter sans la remplacer.</param>
    /// <returns>Liste en lecture seule de descriptions de lecteurs, avec leurs emplacements et leurs contraintes.</returns>
    /// <remarks>La liste décrit les lecteurs ; elle reste distincte des médias actuellement montés,
    /// des emplacements configurés et des réglages supplémentaires des périphériques. L’appelant
    /// conserve les règles de projection et de validation du stockage qui accompagnent ces descriptions.
    /// Aucun transfert de propriété des fichiers référencés n’est effectué par cette opération.</remarks>
    IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);

    /// <summary>Projette les indicateurs du cœur vers les emplacements de médias de la configuration.</summary>
    /// <param name="configuration">Configuration complète empruntée, donnant le modèle, l'adaptateur et les lecteurs configurés.</param>
    /// <param name="ledStates">États des LED indexés selon le protocole du cœur, consultés sans modification.</param>
    /// <returns>Activité par emplacement de média ; un résultat vide signifie qu'aucune association n'est annoncée.</returns>
    /// <remarks>Les associations conservent les conventions réelles du cœur ; un indice inconnu ne permet pas de
    /// déduire un lecteur. La consultation n'ouvre aucun média et ne possède ni périphérique ni processus.
    /// La machine emprunte cette facette à son adaptateur et l'utilise pour publier le statut commun.</remarks>
    IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates);

    /// <summary>Prépare un média pour les contraintes de contenu du cœur, avec conversion si nécessaire.</summary>
    /// <param name="media">Description empruntée du média source ; le fichier utilisateur reste sous la responsabilité de son propriétaire.</param>
    /// <param name="conversionDirectory">Répertoire fourni par l'appelant pour les contenus préparés.</param>
    /// <returns>Description utilisable par le cœur, conservée lorsque le contenu ne demande aucune conversion.</returns>
    /// <remarks>La préparation ne monte pas le média. Les fichiers produits restent gérés par l'appelant qui fournit
    /// le répertoire ; le média source n'est ni détruit ni remplacé. L'hôte vérifie l'annulation avant l'appel.</remarks>
    ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory);

    /// <summary>Prépare les montages de la configuration avant la création de la machine.</summary>
    /// <param name="configuration">Configuration empruntée à adapter sans mutation de ses collections.</param>
    /// <param name="conversionDirectory">Répertoire des contenus préparés fourni par l'appelant, éventuellement partagé entre sessions.</param>
    /// <returns>Configuration utilisable par le backend ; même instance sans préparation nécessaire.</returns>
    /// <remarks>L'identité du module, du modèle, de la machine et de l'émulateur choisi est conservée.
    /// Aucun cœur, processus ni périphérique physique n'est créé par cette opération.</remarks>
    Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory);
}
