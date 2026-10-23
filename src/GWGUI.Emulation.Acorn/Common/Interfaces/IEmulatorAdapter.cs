namespace GWGUI.Emulation.Acorn.Common.Interfaces;
internal interface IEmulatorAdapter : IEmulatorConfigurationSelection, IEmulatorFirmwareAdapter, GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorRuntimePolicy, IEmulatorMediaCapabilities, IEmulatorConfigurationValidation, IEmulatorSettings, IEmulatorInputMapper, IEmulatorPointerCapabilities, IEmulatorVideoSettings, IEmulatorSettingsApplication, IEmulatorStorageSettings
{
    /// <summary>Conserve la configuration préparée par Common si le backend ne demande aucune adaptation lors de sa sélection.</summary>
    /// <param name="configuration">Configuration empruntée portant le cœur choisi et les données existantes.</param>
    /// <returns>La même instance ; une implémentation concrète conserve les données qui ne concernent pas ses contraintes.</returns>
    MachineConfiguration IEmulatorConfigurationSelection.SelectConfiguration(MachineConfiguration configuration) => configuration;

    /// <summary>Conserve les réglages préparés par Common lorsque le backend ne demande aucune adaptation supplémentaire.</summary>
    /// <param name="configuration">Configuration empruntée après les traitements généraux du module.</param>
    /// <param name="values">Champs proposés, disponibles pour les adaptations concrètes.</param>
    /// <returns>La même instance sans adaptation ; les implémentations concrètes retournent leur résultat sans modifier la source.</returns>
    MachineConfiguration IEmulatorSettingsApplication.ApplySettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values) => configuration;

    /// <summary>Conserve les dimensions matérielles de Common lorsque le backend ne fournit aucune résolution configurée.</summary>
    /// <param name="configuration">Configuration empruntée à consulter par les spécialisations concrètes.</param>
    /// <returns>Null sans annonce propre au backend ; une implémentation concrète retourne sa taille en pixels.</returns>
    (int Width, int Height)? IEmulatorVideoSettings.GetOutputResolution(MachineConfiguration configuration) => null;

    /// <summary>Ne demande par défaut aucun redémarrage supplémentaire pour un changement de média.</summary>
    /// <remarks>Les règles générales de Common et la politique concrète du backend restent responsables des décisions effectives.</remarks>
    bool IEmulatorRuntimePolicy.RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => false;

    /// <summary>Ne demande par défaut aucun redémarrage supplémentaire pour une option.</summary>
    /// <remarks>Les métadonnées des options et les contraintes concrètes du backend restent appliquées par leur propriétaire.</remarks>
    bool IEmulatorRuntimePolicy.RequiresOptionRestart(MachineConfiguration configuration, string key) => false;

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

    /// <summary>Réutilise la projection Common existante sans changer les associations ni les règles matérielles.</summary>
    EmulationInputSnapshot IEmulatorInputMapper.MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => InputSnapshotFunctions.Apply(snapshot, configuration.Input);
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
