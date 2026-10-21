namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Adaptation des lecteurs et réglages de stockage propres à un backend.</summary>
/// <remarks>Les descriptions servent à construire les réglages de l’application. Les méthodes par défaut
/// permettent de conserver une configuration lorsque le backend n’ajoute aucun réglage de périphérique.
/// Décrire ou appliquer ces réglages ne transfère pas la propriété des fichiers médias.</remarks>
internal interface IEmulatorStorageSettings
{
    /// <summary>Décrit les lecteurs de médias disponibles pour la configuration.</summary>
    /// <param name="configuration">Configuration empruntée indiquant le modèle et les options de stockage.</param>
    /// <returns>Lecteurs logiques avec leurs types de médias, extensions et capacités.</returns>
    IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
    /// <summary>Décrit les paramètres supplémentaires des périphériques de stockage.</summary>
    /// <param name="configuration">Configuration dont les réglages courants doivent être présentés.</param>
    /// <returns>Liste vide par défaut ; l’adaptateur fournit les paramètres lorsqu’il en prend en charge.</returns>
    IReadOnlyList<EmulationStorageDeviceSettings> DescribeDeviceSettings(MachineConfiguration configuration) => [];
    /// <summary>Applique les paramètres de périphériques et retourne la configuration résultante.</summary>
    /// <param name="configuration">Configuration immuable de départ.</param>
    /// <param name="settings">Paramètres choisis par l’appelant, empruntés pendant leur application.</param>
    /// <returns>Configuration résultante ; la méthode par défaut retourne la même instance sans modification.</returns>
    MachineConfiguration ApplyDeviceSettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings) => configuration;
}
