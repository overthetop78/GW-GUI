using CommonStorageSettingsFunctions = GWGUI.Emulation.Thomson.Common.Machines.Common.Functions.StorageSettingsFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Thomson.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using System.IO;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Constants;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Dictionaries;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Services;
using GWGUI.Emulation.Thomson.Common.Services;
using NativeMedia = GWGUI.Emulation.Thomson.Emulators.Theodore.Functions.MediaRuntimeFunctions;
using NativeOptions = GWGUI.Emulation.Thomson.Emulators.Theodore.Functions.RuntimeOptionFunctions;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Factories;
internal sealed class TheodoreMachineFactory : IEmulatorAdapter, IEmulatorMediaCapabilities, IEmulatorSettings, IEmulatorPointerCapabilities
{
    /// <inheritdoc/>
    public IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);
    /// <inheritdoc/>
    public bool SupportsInitialContent => true;

    /// <inheritdoc/>
    public bool SupportsNoContent => true;

    /// <inheritdoc/>
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([]);
    }

    /// <inheritdoc/>
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => CommonStorageSettingsFunctions.Describe(configuration);

    /// <inheritdoc/>
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => CommonStorageSettingsFunctions.Apply(configuration, settings);


    /// <inheritdoc/>
    public bool SupportsPointerCapture(MachineConfiguration configuration) => true;

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);
    public string EmulatorId => EmulatorConstants.Id;
    public string EmulatorKey => Emulator.Theodore.ToString();
    public EmulationEmulatorDefinition Definition { get; } = new(EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey, CoreCatalog.Entry.Models.Select(model => model.ToString()).ToHashSet(StringComparer.Ordinal));

    /// <inheritdoc/>
    public bool SupportsPointer(MachineConfiguration configuration) => true;

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = CoreHostConstants.NativeOperationSuccess;
        if (arguments is not [CoreHostConstants.CommandLineArgument, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName, (path, emulator) => emulator == Emulator.Theodore ? new ExternalCore(path) : throw new ArgumentOutOfRangeException(nameof(emulator)));
        return true;
    }

    public async ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).GetActiveInstallationAsync(Emulator.Theodore, cancellationToken).ConfigureAwait(false);
        return new EmulationEmulatorInstallation(Definition, installation?.InstalledVersion);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).GetAvailableAsync(Emulator.Theodore, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, $"{item.DeclaredVersion} · {item.PublishedUtc?.LocalDateTime:g}", item.DeclaredVersion)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = CoreReleaseFunctions.Releases.SelectRelease(_availableReleases, release, static selected => selected.Emulator == Emulator.Theodore);
        var forwarded = CoreReleaseFunctions.Releases.CreateInstallProgress<CoreInstallProgress>(progress, static value => value.Fraction);
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).InstallAsync(selected, forwarded, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken) => await new CoreProvider(new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get)).FindInstalledPathAsync(Emulator.Theodore, cancellationToken).ConfigureAwait(false);
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray();
    public IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration) => GWGUI.Emulation.Thomson.Emulators.Theodore.Constants.MediaRuntimeConstants.CartridgeExtensions;
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices(configuration);
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => GWGUI.Emulation.Thomson.Emulators.Theodore.Functions.SettingsDescriptionFunctions.Create(configuration);
    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ConfigurationFunctions.Validate(configuration);
        if (configuration.Core != Emulator.Theodore || !CoreCatalog.Entry.Models.Contains(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();
        IEmulatorCore CreateCore() => OperatingSystem.IsWindows() ? new ProcessCore(context.HostExecutablePath, context.CorePath, Emulator.Theodore) : throw new PlatformNotSupportedException();
        var id = Guid.NewGuid();
        var saveDirectory = context.SaveDirectoryResolver?.Invoke(configuration);
        if (saveDirectory is not null) ((IEmulatorPersistentStorage)this).EnsurePersistentStorage(configuration, saveDirectory);
        var core = CreateCore();
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
                coreFactory: CreateCore,
                supportsPointerCapture: SupportsPointer(configuration),
                supportsNoContent: ((IEmulatorCapabilities)this).SupportsNoContent, inputMapper: this, runtimePolicy: this);
            completed = true;
            return machine;
        }
        finally
        {
            if (!completed)
                core.Dispose();
        }
    }

    /// <inheritdoc/>
    public bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;

    /// <summary>Projette les indicateurs du cœur vers les emplacements de médias de la configuration.</summary>
    /// <param name="configuration">Configuration complète empruntée, donnant le modèle, l'adaptateur et les lecteurs configurés.</param>
    /// <param name="ledStates">États des LED indexés selon le protocole du cœur, consultés sans modification.</param>
    /// <returns>Activité par emplacement de média ; un résultat vide signifie qu'aucune association n'est annoncée.</returns>
    /// <remarks>Les associations conservent les conventions réelles du cœur ; un indice inconnu ne permet pas de
    /// déduire un lecteur. La consultation n'ouvre aucun média et ne possède ni périphérique ni processus.
    /// La machine emprunte cette facette à son adaptateur et l'utilise pour publier le statut commun.</remarks>
    public IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates) => new Dictionary<EmulationMediaSlot, bool>();

    /// <summary>Produit le résumé de configuration du modèle et de l'adaptateur pour la présentation commune.</summary>
    /// <param name="configuration">Configuration complète empruntée, avec les options, les lecteurs et les associations du modèle.</param>
    /// <returns>Résumé public contenant la clé du nom de machine et ses détails techniques, sans démarrer de cœur.</returns>
    /// <remarks>Les informations conservent les règles du modèle et les particularités annoncées par l'adaptateur.
    /// L'opération décrit la configuration ; elle ne crée ni machine en cours d'exécution ni ressource graphique.</remarks>
    public EmulationConfigurationSummary Summarize(MachineConfiguration configuration) => ConfigurationSummaryFunctions.Create(configuration);

    /// <summary>Fournit les commandes du clavier principal par les définitions Common du modèle.</summary>
    /// <param name="configuration">Configuration empruntée du profil et du backend.</param>
    /// <returns>Définitions existantes ; liste vide sans clavier principal.</returns>
    public IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration) => InputSettingsFunctions.KeyboardDefinitions(configuration);

    /// <summary>Conserve les associations Common lorsque le backend ne demande aucune adaptation supplémentaire.</summary>
    /// <param name="configuration">Configuration empruntée à transmettre sans mutation.</param>
    /// <returns>Même instance de configuration ; les fabriques dérivées peuvent adapter leurs contraintes.</returns>
    public MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration) => configuration;

    /// <summary>Conserve le média lorsque le backend consomme directement le format sélectionné.</summary>
    /// <param name="media">Description du média source, empruntée sans ouverture ni modification du fichier.</param>
    /// <param name="conversionDirectory">Répertoire de préparation fourni par la session, inutilisé sans conversion.</param>
    /// <returns>Même description de média ; les fabriques dérivées peuvent fournir leur conversion réelle.</returns>
    public ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory) => ValueTask.FromResult(media);

    /// <summary>Conserve la configuration lorsque ses montages ne demandent aucune conversion préalable.</summary>
    /// <param name="configuration">Configuration empruntée et conservée sans mutation.</param>
    /// <param name="conversionDirectory">Répertoire de préparation fourni par la session, inutilisé sans conversion.</param>
    /// <returns>Tâche terminée contenant la même configuration.</returns>
    public Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory) => Task.FromResult(configuration);
}