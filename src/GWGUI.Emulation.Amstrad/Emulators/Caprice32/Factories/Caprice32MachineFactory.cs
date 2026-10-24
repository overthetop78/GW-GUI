using CommonStorageSettingsFunctions = GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions.StorageSettingsFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Amstrad.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;
using ReleaseConstants = GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants.CoreReleaseConstants;
using EmulatorConstants = GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants.EmulatorConstants;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;
using System.IO;

namespace GWGUI.Emulation.Amstrad.Emulators.Caprice32.Factories;

internal sealed class Caprice32MachineFactory : IEmulatorAdapter, IEmulatorSettings, IEmulatorCapabilities
{
    /// <inheritdoc/>
    /// <remarks>Le firmware de ce backend est intégré ; les adaptations à firmware sélectionnable redéfinissent cette inspection.</remarks>
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([]);
    }

    /// <inheritdoc/>
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => CommonStorageSettingsFunctions.DescribeBasic(configuration);

    /// <inheritdoc/>
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => CommonStorageSettingsFunctions.ApplyBasic(configuration, settings);


    /// <inheritdoc/>
    public bool SupportsPointerCapture(MachineConfiguration configuration) => SupportsPointer(configuration) && ModelCatalog.Get(configuration.Model).MouseButtonCount > BufferConstants.EmptyCollectionCount;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options ?? new Dictionary<string, string>();

    public bool SupportsPointer(MachineConfiguration configuration) => true;
    private static readonly CoreDefinition Core = new(EmulatorConstants.Id,
        EmulatorConstants.LibraryName, ReleaseConstants.LibraryFile, EmulatorConstants.CoreHostCommand,
        new(ReleaseConstants.OfficialPackageUrl), ReleaseConstants.LatestReleaseDisplayName,
        ReleaseConstants.ReleaseProviderSuffix, ReleaseConstants.Architecture,
        CoreLibraryVerifier.VerifyWindowsX64Library, GWGUI.Emulation.Amstrad.Emulators.Caprice32.Dictionaries.KeyboardDictionary.NativeKeys);

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Functions.CoreSettingsDescriptionFunctions.Create(configuration, context is null ? [] : GetSettingsOptions(configuration, context));
    public string EmulatorId => EmulatorConstants.Id;
    public Emulator Emulator => Emulator.Caprice32;
    public string EmulatorKey => Emulator.ToString();
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => [];
    public EmulationEmulatorDefinition Definition { get; } = new( EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey, MachineCatalog.All.Select(machine => machine.Id).ToHashSet(StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = EmulationModuleConstants.DefaultHostExitCode;
        if (arguments is not [EmulatorConstants.CoreHostCommand, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName, (configuration, path) => new ExternalCore(path, Core));
        return true;
    }

    public async ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, Emulator, Core.LibraryFile, Core.DownloadUri, Core.LatestReleaseDisplayName, Core.ReleaseProviderSuffix, Core.Architecture, Core.VerifyInstallation).GetActiveInstallationAsync(Emulator, cancellationToken).ConfigureAwait(false);
        return new EmulationEmulatorInstallation(Definition, installation?.InstalledVersion);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, Emulator, Core.LibraryFile, Core.DownloadUri, Core.LatestReleaseDisplayName, Core.ReleaseProviderSuffix, Core.Architecture, Core.VerifyInstallation).GetAvailableAsync(Emulator, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName ?? item.DeclaredVersion, item.DeclaredVersion, item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = CoreReleaseFunctions.Releases.SelectRelease(_availableReleases, release, selected => selected.Emulator == Emulator);
        var adapter = CoreReleaseFunctions.Releases.CreateInstallProgress<CoreInstallProgress>(progress, static value => value.Fraction);
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, Emulator, Core.LibraryFile, Core.DownloadUri, Core.LatestReleaseDisplayName, Core.ReleaseProviderSuffix, Core.Architecture, Core.VerifyInstallation).InstallAsync(selected, adapter, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken) =>
        await new CoreProvider(new CoreReleaseService(context.HttpClient, context.CoreDirectory, Emulator, Core.LibraryFile, Core.DownloadUri, Core.LatestReleaseDisplayName, Core.ReleaseProviderSuffix, Core.Architecture, Core.VerifyInstallation), Core.VerifyInstallation).FindInstalledPathAsync(Emulator, cancellationToken).ConfigureAwait(false);

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);

    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var native = Caprice32OptionFunctions.ToNative(configuration.EnsureId());
        var saveDirectory = context.SaveDirectoryResolver?.Invoke(configuration);
        if (saveDirectory is not null) ((IEmulatorPersistentStorage)this).EnsurePersistentStorage(configuration, saveDirectory);
        var core = new ProcessCore(context.HostExecutablePath, Core.HostCommand, context.CorePath);
        var completed = false;
        try
        {
            var machine = new Machine(machineId, native,
                core,
                native.Media ?? [], Path.Combine(context.SessionsDirectory, machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
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
}
