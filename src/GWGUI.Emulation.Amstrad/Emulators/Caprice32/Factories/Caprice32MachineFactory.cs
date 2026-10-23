using CommonStorageSettingsFunctions = GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions.StorageSettingsFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Amstrad.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;
using ReleaseConstants = GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants.CoreReleaseConstants;
using EmulatorConstants = GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants.EmulatorConstants;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;
using System.IO;

namespace GWGUI.Emulation.Amstrad.Emulators.Caprice32.Factories;

internal sealed class Caprice32MachineFactory : IEmulatorAdapter, IEmulatorCapabilities
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
    public bool SupportsVideoSettings => true;
    private static readonly CoreDefinition Core = new(EmulatorConstants.Id,
        EmulatorConstants.LibraryName, ReleaseConstants.LibraryFile, EmulatorConstants.CoreHostCommand,
        new(ReleaseConstants.OfficialPackageUrl), ReleaseConstants.LatestReleaseDisplayName,
        ReleaseConstants.ReleaseProviderSuffix, ReleaseConstants.Architecture,
        CoreLibraryVerifier.VerifyWindowsX64Library, GWGUI.Emulation.Amstrad.Emulators.Caprice32.Dictionaries.KeyboardDictionary.NativeKeys);

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, IReadOnlyList<CoreOption> coreOptions) => GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Functions.CoreSettingsDescriptionFunctions.Create(configuration, coreOptions);
    public string EmulatorId => EmulatorConstants.Id;
    public string EmulatorKey => Emulator.Caprice32.ToString();
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => [];
    public EmulationEmulatorDefinition Definition { get; } = new( EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey, MachineCatalog.All.Select(machine => machine.Id).ToHashSet(StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = EmulationModuleConstants.DefaultHostExitCode;
        if (arguments is not [EmulatorConstants.CoreHostCommand, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName, path => new ExternalCore(path, Core));
        return true;
    }

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var service = new CoreReleaseService(context.HttpClient, context.CoreDirectory, Core);
        var version = service.GetInstalledVersion();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition, version));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, Core).GetAvailableAsync(cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName, item.Id, item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = CoreReleaseFunctions.Releases.SelectRelease(_availableReleases, release);
        return await new CoreReleaseService(context.HttpClient, context.CoreDirectory, Core).InstallAsync(selected, progress, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken) => new(new CoreProvider(context.HttpClient, context.CoreDirectory, Core).FindInstalledPathAsync(cancellationToken));

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
}
