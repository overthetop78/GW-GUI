namespace GWGUI.Emulation.Fairchild.Common.Interfaces;
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorMediaCapabilities, IEmulatorConfigurationValidation, IEmulatorSettings, IEmulatorPointerCapabilities
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

    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
