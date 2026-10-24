using CommonFirmwareInspection = GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions.FirmwareConfigurationFunctions;
using CommonStorageSettingsFunctions = GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions.StorageSettingsFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Nintendo.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Functions;
using System.IO;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

internal abstract class CoreMachineFactory : IEmulatorAdapter, IEmulatorSettings, IEmulatorFirmwareAdapter
{
    /// <inheritdoc/>
    public virtual IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => CoreDefinition.Options;

    /// <inheritdoc/>
    public virtual ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(MachineConfiguration configuration, string directory, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directory)) return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([]);
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(Directory.EnumerateFiles(directory, FirmwareCatalogConstants.SearchPattern, SearchOption.AllDirectories)
            .SelectMany(path => ScanFirmwareFileAsync(configuration, path, cancellationToken).GetAwaiter().GetResult()).ToArray());
    }


    /// <inheritdoc/>
    public virtual void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Functions.FirmwareFunctions.Prepare(configuration, systemDirectory);


    /// <inheritdoc/>
    public virtual ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => CommonFirmwareInspection.ScanFileAsync(configuration, path, this, cancellationToken);

    /// <inheritdoc/>
    public virtual EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => CommonStorageSettingsFunctions.Describe(configuration, this);

    /// <inheritdoc/>
    public virtual MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => CommonStorageSettingsFunctions.Apply(configuration, settings);


    /// <inheritdoc/>
    public virtual bool SupportsPointer(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).MouseButtonCount > 0;
    /// <inheritdoc/>
    public virtual bool SupportsPointerCapture(MachineConfiguration configuration) => SupportsPointer(configuration);

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options ?? new Dictionary<string, string>();

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    protected abstract CoreDefinition CoreDefinition { get; }
    protected CoreReleaseSettings ReleaseSettings => CoreDefinition.ReleaseSettings;
    public IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) => CoreDefinition.Firmware.Where(slot => slot.MachineIds is null || slot.MachineIds.Contains(configuration.Model)).ToArray();
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => CoreSettingsFunctions.Blocks(configuration);
    public IReadOnlyList<string> SupportedContentExtensions => CoreDefinition.Extensions;
    public string EmulatorId => CoreDefinition.Id;
    /// <summary>Identité de cœur fournie par le backend au contrat de publication Common.</summary>
    public abstract Emulator Emulator { get; }
    public string EmulatorKey => EmulatorId;
    public EmulationEmulatorDefinition Definition => CoreDefinition.Emulator;

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is not [CoreHostConstants.CommandLineArgument, var pipeName, var videoMapName]) return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName, static (configuration, corePath) => new ExternalCore(corePath));
        return true;
    }

    public async ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var installation = await ReleaseService(context).GetActiveInstallationAsync(Emulator, cancellationToken).ConfigureAwait(false);
        return new EmulationEmulatorInstallation(Definition, installation?.InstalledVersion);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await ReleaseService(context).GetAvailableAsync(Emulator, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName ?? item.DeclaredVersion, item.DeclaredVersion, item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = CoreReleaseFunctions.Releases.SelectRelease(_availableReleases, release, selected => selected.Emulator == Emulator);
        var adapter = CoreReleaseFunctions.Releases.CreateInstallProgress<CoreInstallProgress>(progress, static value => value.Fraction);
        var installation = await ReleaseService(context).InstallAsync(selected, adapter, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken) =>
        await new CoreProvider(ReleaseService(context), CoreReleaseService.VerifyWindowsX64Library).FindInstalledPathAsync(Emulator, cancellationToken).ConfigureAwait(false);

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);

    public virtual Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var native = Configure(configuration.EnsureId());
        var sessionDirectory = Path.Combine(context.SessionsDirectory, machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        var saveDirectory = context.SaveDirectoryResolver?.Invoke(configuration);
        if (saveDirectory is not null) ((IEmulatorPersistentStorage)this).EnsurePersistentStorage(configuration, saveDirectory);
        ProcessCore? core = null;
        var ownershipTransferred = false;
        try
        {
            core = new ProcessCore(context.HostExecutablePath, context.CorePath);
            var machine = new Machine(machineId, native, core, native.Media ?? [], sessionDirectory, null, saveDirectory, inputMapper: this, audioOutputFactory: context.AudioOutputFactory, runtimePolicy: this);
            ownershipTransferred = true;
            return machine;
        }
        finally
        {
            if (!ownershipTransferred)
            {
                core?.Dispose();
            }
        }
    }

    protected virtual MachineConfiguration Configure(MachineConfiguration configuration) => CoreSettingsFunctions.Configure(configuration);

    private CoreReleaseService ReleaseService(EmulatorManagementContext context) => new(context.HttpClient, context.CoreDirectory, Emulator, ReleaseSettings);

    /// <summary>Projette les indicateurs du cœur vers les emplacements de médias de la configuration.</summary>
    /// <param name="configuration">Configuration complète empruntée, donnant le modèle, l'adaptateur et les lecteurs configurés.</param>
    /// <param name="ledStates">États des LED indexés selon le protocole du cœur, consultés sans modification.</param>
    /// <returns>Activité par emplacement de média ; un résultat vide signifie qu'aucune association n'est annoncée.</returns>
    /// <remarks>Les associations conservent les conventions réelles du cœur ; un indice inconnu ne permet pas de
    /// déduire un lecteur. La consultation n'ouvre aucun média et ne possède ni périphérique ni processus.
    /// La machine emprunte cette facette à son adaptateur et l'utilise pour publier le statut commun.</remarks>
    public virtual IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates) => EmulationMediaActivityFunctions.FromLedStates(ledStates);

    /// <summary>Produit le résumé de configuration du modèle et de l'adaptateur pour la présentation commune.</summary>
    /// <param name="configuration">Configuration complète empruntée, avec les options, les lecteurs et les associations du modèle.</param>
    /// <returns>Résumé public contenant la clé du nom de machine et ses détails techniques, sans démarrer de cœur.</returns>
    /// <remarks>Les informations conservent les règles du modèle et les particularités annoncées par l'adaptateur.
    /// L'opération décrit la configuration ; elle ne crée ni machine en cours d'exécution ni ressource graphique.</remarks>
    public virtual EmulationConfigurationSummary Summarize(MachineConfiguration configuration) => ConfigurationSummaryFunctions.Create(configuration);

    /// <summary>Fournit les commandes du clavier principal par les définitions Common du modèle.</summary>
    /// <param name="configuration">Configuration empruntée du profil et du backend.</param>
    /// <returns>Définitions existantes ; liste vide sans clavier principal.</returns>
    public virtual IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration) => InputSettingsFunctions.KeyboardDefinitions(configuration);

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
}
