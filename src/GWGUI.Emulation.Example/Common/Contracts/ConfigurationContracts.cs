using GWGUI.Emulation.Example.Common.Constants;

namespace GWGUI.Emulation.Example.Common.Contracts;

/// <summary>Configuration immuable de la machine, conservée par Common et échangée via IEmulationConfiguration.</summary>
/// <param name="Id">Identité persistante non vide, utilisée pour le sous-dossier de configuration ; jamais un chemin libre.</param>
/// <param name="Volume">Volume de la sortie en pourcentage entier, compris entre 0 et 100 ; il ne change pas les octets du média.</param>
/// <param name="Media">Montages de départ. Null signifie aucun média. La machine copie cette liste à sa construction.</param>
/// <param name="Input">Associations de clavier, souris et ports manette enregistrées avec la configuration ; null demande les valeurs par défaut.</param>
/// <remarks>
/// Le nom MachineConfiguration est commun aux familles : seul le namespace identifie le module.
/// Une configuration décrit une machine, pas une instance de cœur en cours d’exécution. Engine valide les valeurs
/// avant de construire le runtime ; ConfigurationStore écrit le document par l’API de persistance commune.
/// Une modification produit une nouvelle valeur avec « with » : elle ne modifie pas une machine déjà créée.
/// </remarks>
/// <example><code>
/// var current = new MachineConfiguration(Guid.NewGuid(), Volume: 50);
/// var changed = current with { Volume = 25 }; // même identité, nouveau réglage
/// </code></example>
internal sealed record MachineConfiguration(Guid Id, int Volume = 50, IReadOnlyList<EmulationMedia>? Media = null, EmulationInputSettings? Input = null) : IEmulationConfiguration
{
    /// <summary>Version du schéma de persistance, contrôlée par le magasin et par Engine avant utilisation.</summary>
    public int SchemaVersion { get; init; } = ConfigurationStoreConstants.CurrentSchemaVersion;
    /// <summary>Identifiant invariant de famille ; il reste indépendant du nom affiché et de la langue.</summary>
    public string ModuleId => EmulationModuleConstants.ModuleId;
    /// <summary>Identifiant du modèle dans le catalogue de cette famille, vérifié par Engine.</summary>
    public string MachineId => "example";
}
