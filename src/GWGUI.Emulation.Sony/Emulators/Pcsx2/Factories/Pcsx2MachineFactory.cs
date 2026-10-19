using CoreReleaseService = GWGUI.Emulation.Sony.Emulators.Common.Interop.Services.CoreReleaseService;
using FirmwareConstants = GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants.FirmwareConstants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Services;
using System.IO;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;

internal sealed class Pcsx2MachineFactory : IEmulatorAdapter, IEmulatorFirmwareAdapter, IEmulatorSettingsAdapter, IEmulatorControllerAdapter, IEmulatorControllerFeedbackAdapter, IEmulatorMediaAdapter
{
    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public bool RequiresExternalFirmware => true;
    public IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) => FirmwareConstants.All;
    public IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration) => FileSystemFunctions.Describe(configuration,
        Emulators.Common.Interop.Functions.CoreSettingsFunctions.Blocks(Dictionaries.OptionCatalog.All, Pcsx2OptionFunctions.ToNative(configuration)));
    public void ValidateConfiguration(MachineConfiguration configuration) => FileSystemFunctions.Validate(configuration);
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => Pcsx2OptionFunctions.ToNative(configuration).Options!;
    public MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi) =>
        Pcsx2OptionFunctions.ApplyVideoSettings(configuration, graphicsApi);
    public IReadOnlyList<ControllerType> GetControllerTypes(MachineConfiguration configuration) =>
        [ControllerType.Joystick, ControllerType.Keyboard, ControllerType.Mouse,
            ControllerType.KeyboardAndMouse, ControllerType.None];
    public IReadOnlyList<ControllerType> GetUsbControllerTypes(MachineConfiguration configuration, int port) =>
        port is 0 or 1 ? [ControllerType.None, ControllerType.Keyboard, ControllerType.Mouse] : [ControllerType.None];
    public IReadOnlyList<EmulationControllerOutputDefinition> GetControllerOutputs(MachineConfiguration configuration, ControllerType type) =>
        type == ControllerType.Joystick ? InputSettingsFunctions.RumbleOutputs : [];
    public IReadOnlyList<string> SupportedContentExtensions => FirmwareConstants.ContentExtensions;
    public IReadOnlyList<EmulationMediaDevice> GetStorageDevices(MachineConfiguration configuration) => Functions.StorageFunctions.Describe(configuration);
    public string EmulatorId => Pcsx2Constants.Id;
    public string EmulatorKey => Pcsx2Constants.Id;
    public EmulationEmulatorDefinition Definition { get; } = new(Pcsx2Constants.Id, Pcsx2Constants.DisplayName, Pcsx2Constants.DescriptionResourceKey, new[] { ModelConstants.PlayStation2 }.ToHashSet(StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is not [Pcsx2Constants.CoreHostCommand, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName);
        return true;
    }

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var service = ReleaseService(context);
        var version = service.GetInstalledVersion();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition, version));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await ReleaseService(context).GetAvailableAsync(cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName, item.Id, item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = EmulationCoreReleaseFunctions.SelectRelease(_availableReleases, release);
        return await ReleaseService(context).InstallAsync(selected, progress, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.HttpClient);
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(ReleaseService(context).FindInstalledLibraryPath());
    }

    private static CoreReleaseService ReleaseService(EmulatorManagementContext context) =>
        new(context.HttpClient, context.CoreDirectory, Pcsx2Constants.ReleaseSettings, emulatorId: Pcsx2Constants.Id);

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);

    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var native = Pcsx2OptionFunctions.ToNative(configuration.EnsureId());
        var sessionDirectory = Path.Combine(context.SessionsDirectory, machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        ProcessCore? core = null;
        IAudioOutput? audio = null;
        var ownershipTransferred = false;
        try
        {
            core = new ProcessCore(context.HostExecutablePath, context.CorePath);
            audio = native.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null;
            var machine = new Machine(machineId, native, core, native.Media ?? [], sessionDirectory, audio, context.SaveDirectoryResolver?.Invoke(configuration));
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
}
