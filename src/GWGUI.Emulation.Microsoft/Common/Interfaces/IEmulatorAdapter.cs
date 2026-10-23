namespace GWGUI.Emulation.Microsoft.Common.Interfaces;
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorMediaCapabilities, IEmulatorConfigurationValidation, IEmulatorSettings, IEmulatorInputMapper, IEmulatorPointerCapabilities
{
    /// <summary>Ne fournit par défaut aucun lecteur supplémentaire propre au backend.</summary>
    /// <param name="configuration">Configuration empruntée dont les descriptions concrètes peuvent dépendre.</param>
    /// <returns>Liste vide lorsque le backend ne complète pas les lecteurs généraux du modèle.</returns>
    /// <remarks>Common conserve ses descriptions matérielles. Une implémentation concrète fournit les lecteurs propres au cœur
    /// par ce contrat ; elle ne possède ni les fichiers référencés ni leurs contenus montés.</remarks>
    IReadOnlyList<EmulationMediaDevice> IEmulatorMediaCapabilities.DescribeMediaDevices(MachineConfiguration configuration) => [];

    /// <summary>Conserve les contrôles généraux de Common lorsque le backend ne déclare aucune contrainte supplémentaire.</summary>
    /// <remarks>La fabrique concrète implémente ce contrat pour ajouter ses contrôles ; une fabrique dérivée redéfinit
    /// le validateur de sa base avec override.
    /// Ce point commun ne modifie ni la configuration ni les contenus ; les contrôles généraux restent exécutés par leur propriétaire.</remarks>
    void IEmulatorConfigurationValidation.ValidateConfiguration(MachineConfiguration configuration) { }

    /// <summary>Annonce par défaut l’absence d’entrées de pointage ; le backend déclare sa prise en charge effective.</summary>
    bool IEmulatorPointerCapabilities.SupportsPointer(MachineConfiguration configuration) => false;

    /// <summary>Réutilise les associations Common ; la machine ajoute ensuite sa touche de bascule de session.</summary>
    EmulationInputSnapshot IEmulatorInputMapper.MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => InputSnapshotFunctions.Apply(snapshot, configuration.Input, false);

    /// <summary>Réutilise la description Common existante pour le contrat commun de réglages.</summary>
    IReadOnlyList<EmulationSettingsBlock> IEmulatorSettings.DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context) => SettingsDescriptionFunctions.Create(configuration);
    /// <summary>Projette les réglages validés du module vers les valeurs textuelles attendues pendant l’exécution.</summary>
    /// <param name="configuration">Configuration de machine, conservée immuable par le module.</param>
    /// <returns>Associations de clés et valeurs techniques ; les nombres utilisent un format indépendant de la langue.</returns>
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
