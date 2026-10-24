using CommonStorageSettingsFunctions = GWGUI.Emulation.Bandai.Common.Machines.Common.Functions.StorageSettingsFunctions;
using CommonMediaRuntimeFunctions = GWGUI.Emulation.Bandai.Common.Machines.Common.Functions.MediaRuntimeFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Bandai.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Bandai.Emulators.Common.Contracts;
using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Services;
using System.IO;

namespace GWGUI.Emulation.Bandai.Emulators.Common.Interop.Factories;

internal abstract class MachineFactory(EmulatorCatalogEntry catalogEntry) : IEmulatorAdapter
{
    /// <inheritdoc/>
    public virtual async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => [await GWGUI.Emulation.Bandai.Common.Machines.Common.Functions.FirmwareFunctions.ScanFileAsync(path, cancellationToken).ConfigureAwait(false)];

    /// <inheritdoc/>
    public virtual EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => CommonStorageSettingsFunctions.Describe(configuration);

    /// <inheritdoc/>
    public virtual MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => CommonStorageSettingsFunctions.Apply(configuration, settings);


    /// <inheritdoc/>
    public virtual bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => CommonMediaRuntimeFunctions.RequiresMediaRestart(slot);

    /// <inheritdoc/>
    public abstract IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null);

    /// <inheritdoc/>
    public virtual bool SupportsPointerCapture(MachineConfiguration configuration) => false;

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public EmulatorCatalogEntry CatalogEntry { get; } = catalogEntry;
    public Emulator Emulator => CatalogEntry.Emulator;
    public string EmulatorId => CatalogEntry.Id;
    public string EmulatorKey => Emulator.ToString();
    public EmulationEmulatorDefinition Definition => CoreCatalog.GetDefinition(CatalogEntry);

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = CoreHostConstants.NativeOperationSuccess;
        if (arguments is [CoreHostConstants.CommandLineArgument, var pipeName, var videoMapName])
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            CoreHost.Run(pipeName, videoMapName, (path, emulator) => new ExternalCore(path, emulator));
            return true;
        }
        if (arguments is not [CoreOptionProbeConstants.CommandLineArgument, var corePath, var emulatorText]
            || !Enum.TryParse<Emulator>(emulatorText, out var selected) || selected != Emulator) return false;
        try
        {
            Console.Out.WriteLine(CoreOptionProbe.Inspect(corePath, Emulator).Count);
            exitCode = CoreOptionProbeConstants.SuccessExitCode;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(CoreOptionProbe.DescribeFailure(error));
            exitCode = CoreOptionProbeConstants.FailureExitCode;
        }
        return true;
    }

    public async ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).GetActiveInstallationAsync(Emulator, cancellationToken).ConfigureAwait(false);
        var version = installation?.InstalledVersion;
        return new EmulationEmulatorInstallation(Definition, version);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).GetAvailableAsync(Emulator, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, $"{item.DeclaredVersion} · {item.PublishedUtc?.LocalDateTime:g}", item.DeclaredVersion)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = CoreReleaseFunctions.Releases.SelectRelease(_availableReleases, release, selected => selected.Emulator == Emulator);
        var adapter = CoreReleaseFunctions.Releases.CreateInstallProgress<CoreInstallProgress>(progress, static value => value.Fraction);
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).InstallAsync(selected, adapter, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken) => await new CoreProvider(new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get)).FindInstalledPathAsync(Emulator, cancellationToken).ConfigureAwait(false);

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray();

    public virtual IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options;

    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ConfigurationFunctions.Validate(configuration);
        if (configuration.Core != Emulator || !CatalogEntry.Models.Contains(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        var id = Guid.NewGuid();
        var saveDirectory = context.SaveDirectoryResolver?.Invoke(configuration);
        if (saveDirectory is not null) ((IEmulatorPersistentStorage)this).EnsurePersistentStorage(configuration, saveDirectory);
        var core = new ProcessCore(context.HostExecutablePath, context.CorePath, Emulator);
        var completed = false;
        try
        {
            var machine = new Machine(id, configuration, core,
                Path.Combine(context.SessionsDirectory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
                audioOutputFactory: configuration.AudioEnabled ? context.AudioOutputFactory : null,
                saveDirectory: saveDirectory,
                coreFactory: () => new ProcessCore(context.HostExecutablePath, context.CorePath, Emulator), inputMapper: this, runtimePolicy: this);
            completed = true;
            return machine;
        }
        finally { if (!completed) core.Dispose(); }
    }

    /// <summary>Projette les indicateurs du cœur vers les emplacements de médias de la configuration.</summary>
    /// <param name="configuration">Configuration complète empruntée, donnant le modèle, l'adaptateur et les lecteurs configurés.</param>
    /// <param name="ledStates">États des LED indexés selon le protocole du cœur, consultés sans modification.</param>
    /// <returns>Activité par emplacement de média ; un résultat vide signifie qu'aucune association n'est annoncée.</returns>
    /// <remarks>Les associations conservent les conventions réelles du cœur ; un indice inconnu ne permet pas de
    /// déduire un lecteur. La consultation n'ouvre aucun média et ne possède ni périphérique ni processus.
    /// La machine emprunte cette facette à son adaptateur et l'utilise pour publier le statut commun.</remarks>
    public virtual IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates) => new Dictionary<EmulationMediaSlot, bool>();
}
