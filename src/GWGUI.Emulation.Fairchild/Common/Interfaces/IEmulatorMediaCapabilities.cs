namespace GWGUI.Emulation.Fairchild.Common.Interfaces;

/// <summary>Description des lecteurs logiques proposés par un adaptateur pour une configuration.</summary>
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
}
