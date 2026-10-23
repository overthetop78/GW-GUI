namespace GWGUI.Emulation.Sinclair.Common.Interfaces;

/// <summary>Adaptation des lecteurs et réglages de stockage propres à un backend.</summary>
/// <remarks>Les descriptions servent à construire les réglages de l’application. Les méthodes par défaut
/// permettent de conserver une configuration lorsque le backend n’ajoute aucun réglage de périphérique.
/// Décrire ou appliquer ces réglages ne transfère pas la propriété des fichiers médias.</remarks>
internal interface IEmulatorStorageSettings : IEmulatorMediaCapabilities
{
    /// <summary>Décrit les champs propres à une catégorie de périphérique de stockage.</summary>
    /// <param name="configuration">Configuration empruntée contenant les options courantes du lecteur.</param>
    /// <param name="category">Catégorie du lecteur dont les paramètres doivent être présentés.</param>
    /// <returns>Champs natifs du périphérique, ou une liste vide sans réglage particulier.</returns>
    /// <remarks>Les options globales restent dans les blocs de la machine. Ce point de description
    /// consulte les métadonnées natives sans reconstruire DescribeStorage, qui peut lui-même l'appeler.
    /// Il ne monte aucun média et ne change ni fichier ni configuration.</remarks>
    IReadOnlyList<EmulationSettingsField> GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category);

    /// <summary>Décrit l’ensemble des réglages de stockage suivant les règles du backend.</summary>
    /// <param name="configuration">Configuration dont les lecteurs disponibles, emplacements configurés, médias et paramètres doivent être projetés.</param>
    /// <returns>Description complète du stockage ; distincte de la seule liste de lecteurs donnée par DescribeMediaDevices.</returns>
    EmulationStorageSettings DescribeStorage(MachineConfiguration configuration);
    /// <summary>Applique les choix de stockage complets à une configuration.</summary>
    /// <param name="configuration">Configuration de départ, conservée immuable.</param>
    /// <param name="settings">Emplacements, médias et paramètres choisis par l’appelant.</param>
    /// <returns>Configuration résultante après validation par le backend.</returns>
    /// <remarks>Cette opération couvre le stockage complet ; ApplyDeviceSettings ne porte que sur les paramètres supplémentaires des périphériques.</remarks>
    MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings);

    /// <summary>Adapte la configuration à la sélection de lecteurs, indépendamment de leurs médias.</summary>
    /// <param name="configuration">Configuration de départ, conservée immuable.</param>
    /// <param name="slots">Emplacements choisis par l’appelant, dont la liste doit être copiée avant conservation.</param>
    /// <returns>Configuration adaptée aux contraintes du backend ; la même instance est rendue sans adaptation supplémentaire.</returns>
    /// <remarks>Common conserve les contrôles de médias et de paramètres du stockage complet. Ce point
    /// d’adaptation permet au backend d’appliquer ses contraintes matérielles avant la validation finale.</remarks>
    MachineConfiguration ApplyStorageConfiguration(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots) => configuration;
    /// <summary>Décrit les paramètres supplémentaires des périphériques de stockage.</summary>
    /// <param name="configuration">Configuration dont les réglages courants doivent être présentés.</param>
    /// <returns>Liste vide par défaut ; l’adaptateur fournit les paramètres lorsqu’il en prend en charge.</returns>
    IReadOnlyList<EmulationStorageDeviceSettings> DescribeDeviceSettings(MachineConfiguration configuration) => [];
    /// <summary>Applique les paramètres de périphériques et retourne la configuration résultante.</summary>
    /// <param name="configuration">Configuration immuable de départ.</param>
    /// <param name="settings">Paramètres choisis par l’appelant, empruntés pendant leur application.</param>
    /// <returns>Configuration résultante ; la méthode par défaut retourne la même instance sans modification.</returns>
    MachineConfiguration ApplyDeviceSettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings) => configuration;

    /// <summary>Décrit le nombre de cartes mémoire disponibles pour les options courantes.</summary>
    /// <param name="configuration">Configuration empruntée dont les emplacements sont résolus.</param>
    /// <returns>Nombre d’emplacements utilisables ; zéro lorsqu’aucune carte mémoire n’est prise en charge.</returns>
    /// <remarks>Le calcul de base consulte les lecteurs décrits. Si la construction de cette description dépend elle-même du nombre de cartes, le backend fournit ce nombre directement depuis ses métadonnées pour éviter toute récursion.</remarks>
    int GetMemoryCardCount(MachineConfiguration configuration) => DescribeStorage(configuration).AvailableDevices.Count(device => device.Slot.Category == EmulationMediaCategory.MemoryCard);

    /// <summary>Énumère les indices des cartes mémoire disponibles.</summary>
    /// <param name="configuration">Configuration dont le nombre et l’organisation des emplacements sont consultés.</param>
    /// <returns>Indices réels de la description, y compris les organisations discontinues ; un adaptateur peut préciser une sélection dépendant de ses options.</returns>
    IEnumerable<int> GetMemoryCardIndexes(MachineConfiguration configuration) => DescribeStorage(configuration).AvailableDevices.Where(device => device.Slot.Category == EmulationMediaCategory.MemoryCard).Select(device => device.Slot.Index);

    /// <summary>Décrit le libellé technique particulier d’un emplacement de carte mémoire.</summary>
    /// <param name="index">Indice technique de l’emplacement.</param>
    /// <returns>Libellé particulier, ou null pour employer la présentation commune des emplacements.</returns>
    string? GetMemoryCardDisplayLabel(int index) => null;

    /// <summary>Clé native indiquant l’insertion d’une carte mémoire, ou null lorsque le cœur gère autrement son insertion.</summary>
    string? MemoryCardInsertedOptionKey => null;

    /// <summary>Indique si le protocole impose des noms de dossiers fixes pour ses cartes mémoire.</summary>
    bool HasFixedMemoryCardDirectories => false;

    /// <summary>Résout le dossier imposé pour un emplacement de carte mémoire.</summary>
    /// <param name="index">Indice technique de l’emplacement.</param>
    /// <returns>Nom de dossier imposé par le cœur, ou null si aucun dossier particulier n’est requis.</returns>
    string? GetMemoryCardDirectory(int index) => null;
}
