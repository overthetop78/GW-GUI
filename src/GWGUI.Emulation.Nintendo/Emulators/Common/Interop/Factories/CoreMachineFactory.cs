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
}
