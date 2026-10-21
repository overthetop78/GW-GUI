using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Résout les adaptateurs du module et leur confie la création des machines et les commandes d’hôte.</summary>
/// <remarks>Le catalogue associe les identifiants aux adaptateurs. La création d’une machine reste une opération de
/// l’adaptateur sélectionné ; ce service ne lance pas lui-même la boucle d’émulation.</remarks>
public sealed class Engine
{
    /// <summary>Adaptateurs du catalogue indexés par identifiant exact, avec comparaison ordinale.</summary>
    private readonly IReadOnlyDictionary<string, IEmulatorAdapter> _adapters;

    /// <summary>Construit les adaptateurs du catalogue et vérifie implicitement l’unicité de leurs identifiants.</summary>
    /// <exception cref="ArgumentException">Plusieurs adaptateurs portent le même identifiant.</exception>
    public Engine()
    {
        var adapters = EmulatorCatalog.CreateAdapters();
        _adapters = adapters.ToDictionary(adapter => adapter.EmulatorId, StringComparer.Ordinal);
    }

    /// <summary>Valide la configuration puis demande une machine à l’adaptateur sélectionné.</summary>
    /// <param name="configuration">Configuration du modèle, du cœur et des contenus.</param>
    /// <param name="context">Services et répertoires de création transmis à l’adaptateur.</param>
    /// <returns>Machine créée, dont l’appelant doit gérer le cycle de vie et la libération asynchrone.</returns>
    /// <exception cref="ArgumentNullException">La configuration ou le contexte est null.</exception>
    /// <exception cref="EmulationException">Le catalogue ne propose aucun runtime pour ce modèle.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Aucun adaptateur ne correspond au cœur choisi.</exception>
    internal IEmulatedMachine CreateMachine(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(context);
        ConfigurationFunctions.Validate(configuration);
        if (!ConfigurationFunctions.SupportsRuntime(configuration.Model))
            throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreNotFound, ErrorMessages.CoreFileMissing);
        return _adapters.TryGetValue(EmulatorCatalog.Get(configuration.Core).Id, out var adapter) ? adapter.Create(configuration, context) : throw new ArgumentOutOfRangeException(nameof(configuration));
    }

    /// <summary>Résout l’adaptateur du cœur enregistré dans une configuration.</summary>
    /// <param name="configuration">Configuration contenant le choix du cœur.</param>
    /// <returns>Adaptateur conservé dans ce moteur.</returns>
    /// <exception cref="KeyNotFoundException">L’identifiant du catalogue n’est pas présent parmi les adaptateurs.</exception>
    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) => _adapters[EmulatorCatalog.Get(configuration.Core).Id];
    /// <summary>Résout un choix de cœur typé en passant par son identifiant de catalogue.</summary>
    /// <param name="emulator">Valeur du cœur à rechercher.</param>
    /// <returns>Adaptateur associé à son identifiant.</returns>
    internal IEmulatorAdapter Adapter(Emulator emulator) => Adapter(EmulatorCatalog.Get(emulator).Id);

    /// <summary>Recherche un adaptateur par son identifiant exact.</summary>
    /// <param name="emulatorId">Identifiant invariant du cœur.</param>
    /// <returns>Adaptateur conservé dans le moteur.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Aucun adaptateur n’a cet identifiant.</exception>
    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter) ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId));

    /// <summary>Sélectionne les adaptateurs déclarant prendre en charge un modèle.</summary>
    /// <param name="machineId">Identifiant de modèle déclaré dans les définitions d’adaptateurs.</param>
    /// <returns>Nouvelle liste triée par identifiant d’adaptateur avec comparaison ordinale.</returns>
    internal IReadOnlyList<IEmulatorAdapter> Adapters(string machineId) => _adapters.Values.Where(adapter => adapter.Definition.MachineIds.Contains(machineId)).OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).ToArray();

    /// <summary>Présente la ligne de commande à chaque adaptateur jusqu’à ce que l’un la reconnaisse.</summary>
    /// <param name="arguments">Arguments du processus hôte, empruntés et transmis aux adaptateurs.</param>
    /// <param name="exitCode">Code fourni par l’adaptateur qui traite la commande, sinon code de succès par défaut.</param>
    /// <returns>Vrai si un adaptateur a traité la commande ; faux sinon.</returns>
    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values)
            if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = CoreHostConstants.NativeOperationSuccess;
        return false;
    }
}
