using CoreReleaseFunctions = GWGUI.Emulation.Nec.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using System.IO;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Factories;

internal abstract class CoreMachineFactory : IEmulatorAdapter
{
    /// <summary>Expose les lecteurs déjà calculés par la description complète du stockage.</summary>
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => DescribeStorage(configuration).AvailableDevices;
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreStorageSettingsFunctions.Describe(configuration);
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreStorageSettingsFunctions.Apply(configuration, settings);

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreSettingsDescriptionFunctions.Create(configuration);

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    protected abstract CoreReleaseSettings ReleaseSettings { get; }
    public abstract string EmulatorId { get; }
    public string EmulatorKey => EmulatorId;
    public abstract EmulationEmulatorDefinition Definition { get; }

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is not [ProcessCoreConstants.CoreHost, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName);
        return true;
    }

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition, ReleaseService(context).GetInstalledVersion()));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await ReleaseService(context).GetAvailableAsync(cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName, item.Id, item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = CoreReleaseFunctions.Releases.SelectRelease(_availableReleases, release);
        return await ReleaseService(context).InstallAsync(selected, progress, cancellationToken).ConfigureAwait(false);
    }

    public virtual ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(ReleaseService(context).FindInstalledLibraryPath());
    }
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);

    public virtual Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var native = Configure(configuration.EnsureId());
        var sessionDirectory = Path.Combine(context.SessionsDirectory, machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        PrepareSession(configuration, context, sessionDirectory);
        ProcessCore? core = null;
        var ownershipTransferred = false;
        try
        {
            var definition = CoreCatalog.Get(EmulatorId);
            core = new ProcessCore(context.HostExecutablePath, context.CorePath,
                displayName: definition.TransportDisplayName, errorPrefix: definition.ErrorResourcePrefix);
            var machine = new Machine(machineId, native, core, native.Media ?? [], sessionDirectory, null,
                context.SaveDirectoryResolver?.Invoke(configuration), inputMapper: this, audioOutputFactory: context.AudioOutputFactory);
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

    private CoreReleaseService ReleaseService(EmulatorManagementContext context) => new(context.HttpClient, context.CoreDirectory, ReleaseSettings, CoreCatalog.Get(EmulatorId).ErrorResourcePrefix);
}
