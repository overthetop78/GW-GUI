using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;
using CommonStorageSettingsFunctions = GWGUI.Emulation.Snk.Common.Machines.Common.Functions.StorageSettingsFunctions;
using GWGUI.Emulation.Snk.Emulators.Common.Contracts;
using System.IO;

namespace GWGUI.Emulation.Snk.Emulators.Common.Factories;
internal abstract class MachineFactory(EmulatorCatalogEntry catalogEntry) : IEmulatorAdapter, IEmulatorSettings, IEmulatorFirmwareAdapter, IEmulatorInitialContentSettings, IEmulatorMediaCapabilities, IEmulatorControllerAdapter, IEmulatorPointerCapabilities
{
    /// <summary>Expose les rôles natifs et leurs obligations dans la description de firmware commune.</summary>
    public virtual IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) => FirmwareRoles(configuration.Model)
        .Select(role => new FirmwareSlot(GWGUI.Emulation.Snk.Common.Machines.Common.Functions.FirmwareFunctions.Field(role), string.Empty, "Emulation.Snk.Firmware." + role, IsFirmwareRequired(configuration.Model, role))).ToArray();

    /// <inheritdoc/>
    public virtual MachineConfiguration SelectConfiguration(MachineConfiguration configuration)
    {
        var core = configuration.Core;
        var options = CommonCoreHostFunctions.OptionProcessing.FilterForEmulator(CoreOptionValidationFunctions.ForCore(core).Select(option => option.Key), configuration.Options, SettingsConstants.AudioOutput, SettingsConstants.AudioLatency, VideoAudioSettingsConstants.AudioVolumeOption);
        var controllers = configuration.Input.Controllers?.Select(binding => InputSettingsFunctions.Peripherals(core).Contains(binding.Peripheral) ? binding : binding with { Peripheral = InputSettingsFunctions.DefaultPeripheral(configuration.Model) }).ToArray();
        var result = configuration with
        {
            Core = core,
            Options = options,
            Input = configuration.Input with
            {
                Controllers = controllers
            }
        };
        var devices = DescribeMediaDevices(result);
        var slots = devices.Select(device => device.Slot).ToHashSet();
        if (result.Media.Any(media => !slots.Contains(media.Slot)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, "emulatorId");
        result = result with { ConfiguredSlots = result.ConfiguredSlots.Where(slots.Contains).ToArray() };
        return result;
    }

    /// <inheritdoc/>
    public virtual EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => CommonStorageSettingsFunctions.Describe(configuration);

    /// <inheritdoc/>
    public virtual MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => CommonStorageSettingsFunctions.Apply(configuration, settings);


    /// <inheritdoc/>
    public abstract EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration);

    /// <inheritdoc/>
    public virtual bool SupportsPointerCapture(MachineConfiguration configuration) => false;

    public virtual int GetControllerPortCount(MachineConfiguration configuration) => ConfigurationFunctions.ControllerPortCount(configuration.Model);
    public abstract IReadOnlyList<CoreOption> CoreOptions { get; }

    public abstract IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null);
    public virtual bool IsDeferredOption(MachineConfiguration configuration, string key) => false;
    protected abstract IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model);
    protected abstract bool IsFirmwareRequired(MachineModel model, FirmwareCategory category);
    public abstract ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken);
    public EmulatorCatalogEntry CatalogEntry { get; } = catalogEntry;
    public Emulator Emulator => CatalogEntry.Emulator;
    public string EmulatorId => CatalogEntry.Id;
    public string EmulatorKey => Emulator.ToString();
    public EmulationEmulatorDefinition Definition => new(CatalogEntry.Id, CatalogEntry.LibraryName, CatalogEntry.DescriptionResourceKey ?? $"Emulation.Emulator.{CatalogEntry.Id}.Description", CatalogEntry.Models.Select(model => model.ToString()).ToHashSet(StringComparer.Ordinal));

    public abstract bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode);
    public abstract ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken);
    public abstract ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken);
    public abstract ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken);
    public abstract ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken);
    protected abstract IEmulatorCore CreateCore(MachineConfiguration configuration, EmulatorCreationContext context);
    public virtual bool SupportsPointer(MachineConfiguration configuration) => false;
    protected virtual bool RequiresResetRestart => false;
    /// <summary>Les cœurs acceptent un contenu initial sélectionné séparément dans les réglages.</summary>
    public bool SupportsInitialContent => true;
    public virtual bool SupportsNoContent => false;

    public abstract IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
    public abstract IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration);
    public abstract bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path);
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray();
    public virtual IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options;
    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ConfigurationFunctions.Validate(configuration);
        if (configuration.Core != Emulator || !CatalogEntry.Models.Contains(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        var id = Guid.NewGuid();
        var saveDirectory = context.SaveDirectoryResolver?.Invoke(configuration);
        if (saveDirectory is not null) ((IEmulatorPersistentStorage)this).EnsurePersistentStorage(configuration, saveDirectory);
        var core = CreateCore(configuration, context);
        var completed = false;
        try
        {
            var machine = new Machine(
                id,
                configuration,
                core,
                Path.Combine(context.SessionsDirectory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
                audioOutputFactory: configuration.AudioEnabled ? context.AudioOutputFactory : null,
                saveDirectory: saveDirectory,
                coreFactory: () => CreateCore(configuration, context),
                supportsPointerCapture: SupportsPointer(configuration),
                requiresResetRestart: RequiresResetRestart,
                supportsNoContent: ((IEmulatorCapabilities)this).SupportsNoContent,
                inputMapper: this, runtimePolicy: this
            );
            completed = true;
            return machine;
        }
        finally
        {
            if (!completed)
                core.Dispose();
        }
    }

    /// <summary>Projette les indicateurs du cœur vers les emplacements de médias de la configuration.</summary>
    /// <param name="configuration">Configuration complète empruntée, donnant le modèle, l'adaptateur et les lecteurs configurés.</param>
    /// <param name="ledStates">États des LED indexés selon le protocole du cœur, consultés sans modification.</param>
    /// <returns>Activité par emplacement de média ; un résultat vide signifie qu'aucune association n'est annoncée.</returns>
    /// <remarks>Les associations conservent les conventions réelles du cœur ; un indice inconnu ne permet pas de
    /// déduire un lecteur. La consultation n'ouvre aucun média et ne possède ni périphérique ni processus.
    /// La machine emprunte cette facette à son adaptateur et l'utilise pour publier le statut commun.</remarks>
    public virtual IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates) => new Dictionary<EmulationMediaSlot, bool>();

    /// <summary>Produit le résumé de configuration du modèle et de l'adaptateur pour la présentation commune.</summary>
    /// <param name="configuration">Configuration complète empruntée, avec les options, les lecteurs et les associations du modèle.</param>
    /// <returns>Résumé public contenant la clé du nom de machine et ses détails techniques, sans démarrer de cœur.</returns>
    /// <remarks>Les informations conservent les règles du modèle et les particularités annoncées par l'adaptateur.
    /// L'opération décrit la configuration ; elle ne crée ni machine en cours d'exécution ni ressource graphique.</remarks>
    public virtual EmulationConfigurationSummary Summarize(MachineConfiguration configuration) => ConfigurationSummaryFunctions.Create(configuration);

    /// <summary>Fournit les commandes du clavier principal par les définitions Common du modèle.</summary>
    /// <param name="configuration">Configuration empruntée du profil et du backend.</param>
    /// <returns>Définitions existantes ; liste vide sans clavier principal.</returns>
    public virtual IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration) => [];

    /// <summary>Conserve les associations Common lorsque le backend ne demande aucune adaptation supplémentaire.</summary>
    /// <param name="configuration">Configuration empruntée à transmettre sans mutation.</param>
    /// <returns>Même instance de configuration ; les fabriques dérivées peuvent adapter leurs contraintes.</returns>
    public virtual MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration) => configuration;

    /// <summary>Conserve le média lorsque le backend consomme directement le format sélectionné.</summary>
    /// <param name="media">Description du média source, empruntée sans ouverture ni modification du fichier.</param>
    /// <param name="conversionDirectory">Répertoire de préparation fourni par la session, inutilisé sans conversion.</param>
    /// <returns>Même description de média ; les fabriques dérivées peuvent fournir leur conversion réelle.</returns>
    public virtual ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory) => ValueTask.FromResult(media);

    /// <summary>Conserve la configuration lorsque ses montages ne demandent aucune conversion préalable.</summary>
    /// <param name="configuration">Configuration empruntée et conservée sans mutation.</param>
    /// <param name="conversionDirectory">Répertoire de préparation fourni par la session, inutilisé sans conversion.</param>
    /// <returns>Tâche terminée contenant la même configuration.</returns>
    public virtual Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory) => Task.FromResult(configuration);

    /// <summary>Conserve les formats de cartouche déjà décrits par le modèle Common.</summary>
    /// <param name="configuration">Configuration empruntée du modèle et du backend.</param>
    /// <returns>Null en l'absence de restriction supplémentaire ; les fabriques dérivées peuvent préciser leurs contraintes.</returns>
    public virtual IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration) => null;
}