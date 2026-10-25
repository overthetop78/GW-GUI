namespace GWGUI.Emulation.Commodore.Common.Interfaces;

/// <summary>Application des valeurs d’éditeurs par l’adaptateur choisi pour la configuration.</summary>
/// <remarks>Common transmet les champs à cet adaptateur puis conserve les règles de validation de la configuration.
/// Le backend complète les traitements Common suivant ses propres réglages ; sans adaptation supplémentaire, la configuration reçue est conservée.</remarks>
internal interface IEmulatorSettingsApplication
{
    /// <summary>Applique les valeurs de réglages et retourne la configuration résultante.</summary>
    /// <param name="configuration">Configuration initiale empruntée, dont l’identité et les données étrangères aux réglages sont conservées.</param>
    /// <param name="values">Valeurs proposées par l’hôte, indexées par les identifiants des champs ; null représente une valeur non renseignée.</param>
    /// <returns>Configuration résultant du traitement de l’adaptateur, soumise aux contrôles Common du module.</returns>
    /// <remarks>L’application de ces champs prépare des données de configuration. Le propriétaire du runtime
    /// reste responsable de transmettre les changements à une session active et d’organiser ses redémarrages nécessaires.</remarks>
    MachineConfiguration ApplySettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values);
}
