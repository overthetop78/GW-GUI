using CommonStorageSettingsFunctions = GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions.StorageSettingsFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Amstrad.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;
using ReleaseConstants = GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Constants.CoreReleaseConstants;
using EmulatorConstants = GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Constants.EmulatorConstants;
using System.IO;

namespace GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Factories;

internal sealed class CrocoDSMachineFactory : IEmulatorAdapter, IEmulatorSettings, IEmulatorCapabilities, IEmulatorConfigurationSelection, IEmulatorConfigurationValidation, IEmulatorStorageSettings
{
    /// <inheritdoc/>
    /// <remarks>Le firmware de ce backend est intégré ; les adaptations à firmware sélectionnable redéfinissent cette inspection.</remarks>
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([]);
    }

    /// <inheritdoc/>
    public bool SupportsPointerCapture(MachineConfiguration configuration) => SupportsPointer(configuration) && ModelCatalog.Get(configuration.Model).MouseButtonCount > BufferConstants.EmptyCollectionCount;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options ?? new Dictionary<string, string>();

    public void ValidateConfiguration(MachineConfiguration configuration) => Validate(configuration);

    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration)
    {
        var device = new EmulationMediaDevice(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy,
            [StorageSettingsFunctionsConstants.Dsk, StorageSettingsFunctionsConstants.Sna, StorageSettingsFunctionsConstants.Kcr], RequiresMachineRecreation: true,
            DisplayLabel: StorageSettingsFunctionsConstants.FloppyDriveLabel, IsPermanent: true);
        return new EmulationStorageSettings([device], [EmulationMediaSlot.Floppy0], EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []));
    }

    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        configuration = CommonStorageSettingsFunctions.ApplyBasic(configuration, settings);
        if (settings.ConfiguredSlots.Any(slot => slot != EmulationMediaSlot.Floppy0)) throw new ArgumentOutOfRangeException(nameof(settings));
        Validate(configuration);
        return configuration;
    }

    public MachineConfiguration SelectConfiguration(MachineConfiguration configuration) => configuration with
    {
        Media = (configuration.Media ?? []).Where(item => item.Category is MediaCategory.Floppy or MediaCategory.Snapshot && EmulatorConstants.Extensions.Contains(Path.GetExtension(item.Path))).Take(EmulatorConstants.MaximumInsertedMediaCount).ToArray(),
        Options = (configuration.Options ?? new Dictionary<string, string>()).Where(item => item.Key.StartsWith(EmulatorConstants.OptionKeyPrefix, StringComparison.Ordinal))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal)
    };
    public bool SupportsPointer(MachineConfiguration configuration) => false;
    public bool SupportsVideoSettings => false;
    private static readonly CoreDefinition Core = new(EmulatorConstants.Id,
        EmulatorConstants.LibraryName, ReleaseConstants.LibraryFile, EmulatorConstants.CoreHostCommand,
        new(ReleaseConstants.OfficialPackageUrl), ReleaseConstants.LatestReleaseDisplayName,
        ReleaseConstants.ReleaseProviderSuffix, ReleaseConstants.Architecture,
        CoreLibraryVerifier.VerifyWindowsX64Library, GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Dictionaries.KeyboardDictionary.NativeKeys);

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Functions.CoreSettingsDescriptionFunctions.Create(configuration, context is null ? [] : GetSettingsOptions(configuration, context));
    public string EmulatorId => EmulatorConstants.Id;
    public Emulator Emulator => Emulator.CrocoDS;
    public string EmulatorKey => Emulator.ToString();
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => context is null ? [] : CoreOptionsReader.Read(Path.Combine(context.CoreDirectory, Core.LibraryFile));
    public EmulationEmulatorDefinition Definition { get; } = new(EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey, new HashSet<string>([EmulatorConstants.MachineId], StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = EmulationModuleConstants.DefaultHostExitCode;
        if (arguments is not [var command, var pipe, var video]
            || command != Core.HostCommand) return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipe, video, (configuration, path) => new ExternalCore(path, Core));
        return true;
    }

    private static CoreReleaseService Releases(EmulatorManagementContext context) => new(context.HttpClient, context.CoreDirectory, Emulator.CrocoDS, Core.LibraryFile, Core.DownloadUri, Core.LatestReleaseDisplayName, Core.ReleaseProviderSuffix, Core.Architecture, Core.VerifyInstallation);

    public async ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var installation = await Releases(context).GetActiveInstallationAsync(Emulator, cancellationToken).ConfigureAwait(false);
        return new EmulationEmulatorInstallation(Definition, installation?.InstalledVersion);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await Releases(context).GetAvailableAsync(Emulator, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName ?? item.DeclaredVersion, item.DeclaredVersion, item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = CoreReleaseFunctions.Releases.SelectRelease(_availableReleases, release, selected => selected.Emulator == Emulator);
        var adapter = CoreReleaseFunctions.Releases.CreateInstallProgress<CoreInstallProgress>(progress, static value => value.Fraction);
        var installation = await Releases(context).InstallAsync(selected, adapter, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken) =>
        await new CoreProvider(Releases(context), Core.VerifyInstallation).FindInstalledPathAsync(Emulator, cancellationToken).ConfigureAwait(false);

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Model != EmulatorConstants.MachineId)
            throw new ArgumentOutOfRangeException(nameof(configuration.Model));
        var media = configuration.Media ?? [];
        if (media.Count(item => item.IsInserted) > EmulatorConstants.MaximumInsertedMediaCount)
            throw new ArgumentOutOfRangeException(nameof(configuration.Media));
        foreach (var item in media)
            if (item.Category is not MediaCategory.Floppy and not MediaCategory.Snapshot
                || !EmulatorConstants.Extensions.Contains(Path.GetExtension(item.Path)))
                throw new ArgumentOutOfRangeException(nameof(configuration.Media));
    }

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration)
    {
        Validate(configuration);
        return EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);
    }

    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        Validate(configuration);
        var id = Guid.NewGuid();
        // Keep only this core's native options; host/device settings are handled by the module.
        var native = configuration.EnsureId() with
        {
            Options = (configuration.Options ?? new Dictionary<string, string>()).Where(item => item.Key.StartsWith(EmulatorConstants.OptionKeyPrefix, StringComparison.Ordinal))
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal)
        };
        var saveDirectory = context.SaveDirectoryResolver?.Invoke(configuration);
        if (saveDirectory is not null) ((IEmulatorPersistentStorage)this).EnsurePersistentStorage(configuration, saveDirectory);
        var core = new ProcessCore(context.HostExecutablePath, Core.HostCommand, context.CorePath, supportsPointerCapture: SupportsPointer(configuration));
        var completed = false;
        try
        {
            var machine = new Machine(id, native, core,
                native.Media ?? [], Path.Combine(context.SessionsDirectory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
                null,
                saveDirectory, inputMapper: this, audioOutputFactory: context.AudioOutputFactory, runtimePolicy: this);
            completed = true;
            return machine;
        }
        finally
        {
            if (!completed) core.Dispose();
        }
    }

    /// <summary>Projette les indicateurs du cœur vers les emplacements de médias de la configuration.</summary>
    /// <param name="configuration">Configuration complète empruntée, donnant le modèle, l'adaptateur et les lecteurs configurés.</param>
    /// <param name="ledStates">États des LED indexés selon le protocole du cœur, consultés sans modification.</param>
    /// <returns>Activité par emplacement de média ; un résultat vide signifie qu'aucune association n'est annoncée.</returns>
    /// <remarks>Les associations conservent les conventions réelles du cœur ; un indice inconnu ne permet pas de
    /// déduire un lecteur. La consultation n'ouvre aucun média et ne possède ni périphérique ni processus.
    /// La machine emprunte cette facette à son adaptateur et l'utilise pour publier le statut commun.</remarks>
    public IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates) => EmulationMediaActivityFunctions.FromLedStates(ledStates);

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

    /// <summary>Conserve les formats de cartouche déjà décrits par le modèle Common.</summary>
    /// <param name="configuration">Configuration empruntée du modèle et du backend.</param>
    /// <returns>Null en l'absence de restriction supplémentaire ; les fabriques dérivées peuvent préciser leurs contraintes.</returns>
    public IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration) => null;
}
