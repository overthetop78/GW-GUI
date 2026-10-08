using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using System.IO;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Factories;

internal abstract class CoreMachineFactory : IEmulatorAdapter
{
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) =>
        GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreStorageSettingsFunctions.Describe(configuration);
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) =>
        GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreStorageSettingsFunctions.Apply(configuration, settings);

    public IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration) =>
        GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreSettingsDescriptionFunctions.Create(configuration);

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    protected abstract CoreReleaseSettings ReleaseSettings { get; }
    public abstract string EmulatorId { get; }
    public string EmulatorKey => EmulatorId;
    public abstract EmulationEmulatorDefinition Definition { get; }

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        return false;
    }

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition,
            ReleaseService(context).GetInstalledVersion()));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await ReleaseService(context).GetAvailableAsync(cancellationToken)
            .ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName,
            item.Id, item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context,
        EmulationEmulatorRelease release, IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        if (!_availableReleases.TryGetValue(release.Id, out var selected))
            throw new ArgumentException(nameof(release));
        return await ReleaseService(context).InstallAsync(selected, progress, cancellationToken)
            .ConfigureAwait(false);
    }

    public ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ReleaseService(context).RequiredLibraryPath;
        if (!File.Exists(path)) return ValueTask.FromResult<string?>(null);
        try
        {
            CoreReleaseService.VerifyWindowsX64Library(path);
            return ValueTask.FromResult<string?>(path);
        }
        catch (IOException) { return ValueTask.FromResult<string?>(null); }
        catch (InvalidDataException) { return ValueTask.FromResult<string?>(null); }
    }

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) =>
        EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);

    public virtual Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var native = Configure(configuration.EnsureId());
        var sessionDirectory = Path.Combine(context.SessionsDirectory,
            machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        return new Machine(machineId, native,
            new ProcessCore(context.HostExecutablePath, context.CorePath),
            native.Media ?? [], sessionDirectory,
            native.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration));
    }

    protected virtual MachineConfiguration Configure(MachineConfiguration configuration) => CoreSettingsFunctions.Configure(configuration);

    private CoreReleaseService ReleaseService(EmulatorManagementContext context) =>
        new(context.HttpClient, context.CoreDirectory, ReleaseSettings);
}
