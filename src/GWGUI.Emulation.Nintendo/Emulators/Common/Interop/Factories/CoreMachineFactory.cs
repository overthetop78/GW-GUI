using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Functions;
using System.IO;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

internal abstract class CoreMachineFactory : IEmulatorAdapter
{
    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    protected abstract CoreDefinition CoreDefinition { get; }
    protected CoreReleaseSettings ReleaseSettings => CoreDefinition.ReleaseSettings;
    public IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) =>
        CoreDefinition.Firmware.Where(slot => slot.MachineIds.Contains(configuration.Model)).ToArray();
    public IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration) =>
        CoreSettingsFunctions.Blocks(configuration);
    public IReadOnlyList<string> SupportedContentExtensions => CoreDefinition.Extensions;
    public string EmulatorId => CoreDefinition.Id;
    public string EmulatorKey => EmulatorId;
    public EmulationEmulatorDefinition Definition => CoreDefinition.Emulator;

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is not [ProcessCoreConstants.CoreHost, var pipeName, var videoMapName]) return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName);
        return true;
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
        ProcessCore? core = null;
        IAudioOutput? audio = null;
        var ownershipTransferred = false;
        try
        {
            core = new ProcessCore(context.HostExecutablePath, context.CorePath);
            audio = native.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null;
            var machine = new Machine(machineId, native, core, native.Media ?? [], sessionDirectory,
                audio, context.SaveDirectoryResolver?.Invoke(configuration));
            ownershipTransferred = true;
            return machine;
        }
        finally
        {
            if (!ownershipTransferred)
            {
                try { core?.Dispose(); }
                finally { audio?.Dispose(); }
            }
        }
    }

    protected virtual MachineConfiguration Configure(MachineConfiguration configuration) => CoreSettingsFunctions.Configure(configuration);

    private CoreReleaseService ReleaseService(EmulatorManagementContext context) =>
        new(context.HttpClient, context.CoreDirectory, ReleaseSettings);
}
