using CommonFirmwareInspection = GWGUI.Emulation.Nec.Common.Machines.Common.Functions.FirmwareFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Nec.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using System.IO;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Factories;

internal abstract class CoreMachineFactory : IEmulatorAdapter, IEmulatorSettings, IEmulatorFirmwareAdapter
{
    /// <inheritdoc/>
    public virtual IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => CoreSettingsFunctions.Options(EmulatorId, configuration);

    /// <inheritdoc/>
    public virtual async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(MachineConfiguration configuration, string directory, CancellationToken cancellationToken)
    {
        var paths = CommonFirmwareInspection.Slots(configuration).Count != 0
            ? Directory.Exists(directory) ? Directory.EnumerateFiles(directory, FirmwareCatalogConstants.SearchPattern, SearchOption.AllDirectories) : []
            : ModelCatalog.Get(configuration.Model).SupportsCdDrive ? FirmwareCatalog.EnumerateCandidates(directory) : [];
        var candidates = new List<EmulationFirmwareCandidate>();
        foreach (var path in paths) candidates.AddRange(await ScanFirmwareFileAsync(configuration, path, cancellationToken).ConfigureAwait(false));
        cancellationToken.ThrowIfCancellationRequested();
        return candidates;
    }


    /// <summary>Décrit le champ de ROM unique déjà présenté par les modèles CD.</summary>
    /// <remarks>Les adaptations possédant un catalogue de fichiers nommés redéfinissent cette méthode.
    /// Le constructeur de champ reste la source de ses identifiants et aides ; aucun appel à DescribeSettings
    /// ne crée de boucle avec la construction des réglages qui consulte également les emplacements.</remarks>
    public virtual IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration)
    {
        if (!ModelCatalog.Get(configuration.Model).SupportsCdDrive) return [];
        var field = CoreSettingsDescriptionFunctions.FirmwarePath(configuration.FirmwarePath);
        return [new FirmwareSlot(field.Id, string.Empty, field.LabelResourceKey, ExplanationResourceKey: field.ExplanationResourceKey, DetailedExplanationResourceKey: field.DetailedExplanationResourceKey)];
    }

    /// <inheritdoc/>
    public virtual void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.FirmwareFunctions.Prepare(configuration, systemDirectory);


    /// <inheritdoc/>
    public virtual ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => CommonFirmwareInspection.ScanFileAsync(configuration, path, cancellationToken);

    /// <inheritdoc/>
    public virtual bool SupportsPointer(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).MouseButtonCount > 0;
    /// <inheritdoc/>
    public virtual bool SupportsPointerCapture(MachineConfiguration configuration) => SupportsPointer(configuration);

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options ?? new Dictionary<string, string>();

    /// <summary>Expose les lecteurs déjà calculés par la description complète du stockage.</summary>
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => DescribeStorage(configuration).AvailableDevices;
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreStorageSettingsFunctions.Describe(configuration);
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreStorageSettingsFunctions.Apply(configuration, settings);

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreSettingsDescriptionFunctions.Create(configuration);

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    protected abstract CoreReleaseSettings ReleaseSettings { get; }
    public abstract string EmulatorId { get; }
    /// <summary>Identité de cœur fournie par le backend au contrat de publication Common.</summary>
    public abstract Emulator Emulator { get; }
    public string EmulatorKey => EmulatorId;
    public abstract EmulationEmulatorDefinition Definition { get; }

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is not [CoreHostConstants.CommandLineArgument, var pipeName, var videoMapName])
            return false;
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

    public virtual async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken) =>
        await new CoreProvider(ReleaseService(context), path => CoreReleaseService.VerifyWindowsX64Library(path, CoreCatalog.Get(EmulatorId).ErrorResourcePrefix)).FindInstalledPathAsync(Emulator, cancellationToken).ConfigureAwait(false);
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);

    public virtual Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var native = Configure(configuration.EnsureId());
        var sessionDirectory = Path.Combine(context.SessionsDirectory, machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        PrepareSession(configuration, context, sessionDirectory);
        var saveDirectory = context.SaveDirectoryResolver?.Invoke(configuration);
        if (saveDirectory is not null) ((IEmulatorPersistentStorage)this).EnsurePersistentStorage(configuration, saveDirectory);
        ProcessCore? core = null;
        var ownershipTransferred = false;
        try
        {
            var definition = CoreCatalog.Get(EmulatorId);
            core = new ProcessCore(context.HostExecutablePath, context.CorePath,
                displayName: definition.TransportDisplayName, errorPrefix: definition.ErrorResourcePrefix);
            var machine = new Machine(machineId, native, core, native.Media ?? [], sessionDirectory, null,
                saveDirectory, inputMapper: this, audioOutputFactory: context.AudioOutputFactory, runtimePolicy: this);
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
    protected virtual void PrepareSession(MachineConfiguration configuration, EmulatorCreationContext context, string sessionDirectory) { }

    protected virtual MachineConfiguration Configure(MachineConfiguration configuration) => CoreSettingsFunctions.Configure(configuration);

    private CoreReleaseService ReleaseService(EmulatorManagementContext context) => new(context.HttpClient, context.CoreDirectory, Emulator, ReleaseSettings, CoreCatalog.Get(EmulatorId).ErrorResourcePrefix);
}
