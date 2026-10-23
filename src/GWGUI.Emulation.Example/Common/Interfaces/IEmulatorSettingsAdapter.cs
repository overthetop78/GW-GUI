namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Description des réglages et options qu’un backend expose dans la configuration de machine.</summary>
/// <remarks>Les blocs sont des données de présentation ; les valeurs restent appliquées par les propriétaires
/// de configuration et de cœur. Les clés de ressources permettent à l’hôte de présenter ces données dans sa langue.</remarks>
internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    /// <summary>Décrit les options natives utilisables pour construire les réglages du backend.</summary>
    /// <param name="configuration">Configuration facultative pour résoudre les options qui dépendent du modèle ou des valeurs choisies.</param>
    /// <param name="context">Contexte facultatif de gestion lorsque la lecture des options requiert l’installation du cœur et ses services.
    /// Un backend qui en a besoin doit le demander à son appelant, sans conserver ni détruire les services empruntés.</param>
    /// <returns>Descriptions d’options, choix disponibles et métadonnées de présentation.</returns>
    IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null);

    /// <summary>Indique si une option est conservée pour une application différée.</summary>
    /// <param name="configuration">Configuration dont les autres choix peuvent conditionner cette option.</param>
    /// <param name="key">Clé native de l’option.</param>
    /// <returns>True lorsque l’option doit être appliquée à une étape ultérieure de préparation.</returns>
    bool IsDeferredOption(MachineConfiguration configuration, string key);

    /// <summary>Résout la quantité de RAM correspondant au modèle et à ses options.</summary>
    /// <param name="configuration">Configuration empruntée à décrire.</param>
    /// <returns>Capacité de mémoire principale en KiB.</returns>
    int GetMemoryKib(MachineConfiguration configuration);

    /// <summary>Décrit les latences audio autorisées pour cette configuration.</summary>
    /// <param name="configuration">Configuration dont les contraintes de sortie sont consultées.</param>
    /// <returns>Choix de latence en millisecondes, conservés dans leur ordre de présentation.</returns>
    IReadOnlyList<int> GetAudioLatencyChoices(MachineConfiguration configuration);

    /// <summary>Résout les réglages audio effectifs, y compris les choix propres au backend.</summary>
    /// <param name="configuration">Configuration empruntée contenant les valeurs enregistrées.</param>
    /// <returns>Réglages audio utilisables par le contrôleur de sortie Common.</returns>
    AudioConfiguration GetAudioConfiguration(MachineConfiguration configuration);

    /// <summary>Applique le choix du moteur graphique aux options propres au backend.</summary>
    /// <param name="configuration">Configuration de départ, conservée immuable.</param>
    /// <param name="graphicsApi">API graphique sélectionnée par l’hôte.</param>
    /// <returns>Configuration résultante après adaptation des valeurs natives du moteur.</returns>
    MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi);

    /// <summary>Identifie un champ qui représente un réglage d’horloge processeur.</summary>
    /// <param name="fieldId">Identifiant technique du champ présenté.</param>
    /// <returns>True pour les réglages de cadence du processeur, false pour les autres champs.</returns>
    bool IsCpuClock(string fieldId);
}
