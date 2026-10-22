namespace GWGUI.Emulation.Amstrad.Common.Interfaces;

/// <summary>Validation des contraintes de configuration propres à un adaptateur.</summary>
/// <remarks>Common vérifie d’abord les règles générales du modèle et de ses réglages. Ce contrat permet
/// à un adaptateur de compléter ces contrôles avec ses propres contraintes, sans démarrer le cœur.
/// La déclaration est identique dans tous les modules ; les règles appliquées restent celles du backend.
/// Une fabrique qui ne fournit pas de validation supplémentaire conserve le parcours général de Common.</remarks>
internal interface IEmulatorConfigurationValidation
{
    /// <summary>Contrôle les contraintes du backend avant que Common utilise la configuration.</summary>
    /// <param name="configuration">Configuration empruntée, conservée sans modification par ce contrôle.</param>
    /// <remarks>Le validateur conserve les diagnostics et exceptions de son implémentation existante.
    /// Il ne remplace ni les contrôles généraux du modèle, ni la préparation des fichiers de session,
    /// ni la validation des options lorsqu’elle est appelée séparément. L’appelant conserve la propriété
    /// de la configuration, des contenus référencés et de toute session déjà ouverte.</remarks>
    void ValidateConfiguration(MachineConfiguration configuration);
}
