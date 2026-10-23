namespace GWGUI.Emulation.Amstrad.Common.Interfaces;

/// <summary>Description des blocs de réglages fournis par un adaptateur pour une configuration.</summary>
/// <remarks>Common assemble ces blocs avec les autres réglages de la machine. Le contrat retourne des données
/// de présentation et ne possède aucune session, sortie audio ou surface vidéo.</remarks>
internal interface IEmulatorSettings
{
    /// <summary>Résout les valeurs audio à transmettre à la sortie possédée par la session.</summary>
    /// <param name="configuration">Configuration empruntée contenant les options ou valeurs audio enregistrées.</param>
    /// <returns>Valeurs de sortie, latence et séparation stéréo, après les adaptations natives existantes.</returns>
    /// <remarks>Cette opération ne crée aucune sortie. Le propriétaire de session utilise les valeurs retournées
    /// pour sa fabrique et conserve la fermeture et la libération de la sortie dans son cycle de vie.</remarks>
    AudioConfiguration GetAudioConfiguration(MachineConfiguration configuration);

    /// <summary>Décrit les choix de latence déjà proposés pour cette configuration.</summary>
    /// <param name="configuration">Modèle, cœur et options déterminant les contraintes audio.</param>
    /// <returns>Latences en millisecondes, dans leur ordre de présentation, sans modifier la collection empruntée.</returns>
    IReadOnlyList<int> GetAudioLatencyChoices(MachineConfiguration configuration);

    /// <summary>Décrit les réglages du backend avec les valeurs courantes de la configuration.</summary>
    /// <param name="configuration">Modèle, options et périphériques empruntés à présenter sans les modifier.</param>
    /// <param name="context">Services de gestion empruntés pour résoudre les options ou métadonnées qui en dépendent ; null si ces services ne sont pas nécessaires.</param>
    /// <returns>Blocs et éditeurs du backend, avec leurs clés de ressources et leurs valeurs courantes.</returns>
    /// <remarks>Les options natives sont résolues par les contrats complémentaires de l’adaptateur lorsqu’il en expose.</remarks>
    IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null);

    /// <summary>Indique si une option sera validée après le chargement de son contenu.</summary>
    /// <param name="configuration">Configuration empruntée dont le contenu détermine les options disponibles.</param>
    /// <param name="key">Clé technique de la valeur configurée.</param>
    /// <returns>True lorsque le cœur annonce les choix de cette option après le chargement ; false conserve la validation habituelle.</returns>
    /// <remarks>Une validation différée est liée à la disponibilité du catalogue natif. Le contrôle final
    /// utilise les valeurs réellement annoncées par le cœur une fois le contenu chargé.</remarks>
    bool IsDeferredOption(MachineConfiguration configuration, string key);

    /// <summary>Reconnaît un champ décrivant le réglage de cadence du processeur par le cœur.</summary>
    /// <param name="fieldId">Identifiant technique du champ décrit par cet adaptateur.</param>
    /// <returns>True pour un champ reconnu par la règle native ; false si aucune règle ne le reconnaît.</returns>
    /// <remarks>La reconnaissance porte sur les métadonnées de réglage et ne change pas la fréquence du cœur.</remarks>
    bool IsCpuClock(string fieldId);
}