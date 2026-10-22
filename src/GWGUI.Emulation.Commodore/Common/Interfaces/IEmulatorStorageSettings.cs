namespace GWGUI.Emulation.Commodore.Common.Interfaces;

/// <summary>Adaptation des lecteurs et réglages de stockage propres à un backend.</summary>
/// <remarks>Les descriptions servent aux réglages de la machine ; les fichiers médias restent sous la responsabilité de leur propriétaire.</remarks>
internal interface IEmulatorStorageSettings : IEmulatorMediaCapabilities
{
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
}
