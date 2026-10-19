using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using System.IO;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sony.Emulators.Common.Functions;
using PlayStationFirmware = GWGUI.Emulation.Sony.Common.Machines.PlayStation.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;

internal abstract class CoreMachineFactory : IEmulatorAdapter, IEmulatorFirmwareAdapter, IEmulatorSettingsAdapter, IEmulatorControllerAdapter, IEmulatorControllerFeedbackAdapter, IEmulatorMediaAdapter
{
    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    protected abstract CoreDefinition CoreDefinition { get; }
    protected CoreReleaseSettings ReleaseSettings => CoreDefinition.ReleaseSettings;
    public bool RequiresExternalFirmware => CoreDefinition.RequiresExternalFirmware;
    public IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) => configuration.Model == ModelConstants.PlayStation ? [PlayStationFirmware.Selected] : CoreDefinition.Firmware;
    public virtual IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration) => CoreSettingsFunctions.Blocks(CoreDefinition, configuration);
    public virtual void ValidateConfiguration(MachineConfiguration configuration) { }
    public virtual IReadOnlyList<EmulationMediaDevice> GetStorageDevices(MachineConfiguration configuration) => [];
    public virtual IReadOnlyList<InputBindingDefinition>? GetControllerBindingDefinitions(MachineConfiguration configuration, ControllerType type, int port) => null;
    public virtual IReadOnlyList<EmulationSettingsBlock> GetMachineSettingsBlocks(MachineConfiguration configuration)
    {
        var globalStorage = CoreDefinition.Options.Where(option => option.IsGlobalStorageOption).Select(option => option.Key).ToHashSet(StringComparer.Ordinal);
        return GetSettingsBlocks(configuration).Select(block => block.Tab == EmulationMachineTab.Storage
            ? block with { Fields = block.Fields.Where(field => globalStorage.Contains(field.Id)).ToArray() }
            : block).Where(block => block.Fields.Count > 0).ToArray();
    }
    public virtual IReadOnlyList<EmulationSettingsField> GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category) =>
        category == EmulationMediaCategory.CompactDiscDrive
            ? CoreSettingsFunctions.Blocks(CoreDefinition.Options.Where(option => !option.IsGlobalStorageOption).ToArray(), configuration).Where(block => block.Tab == EmulationMachineTab.Storage).SelectMany(block => block.Fields).ToArray()
            : [];
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => Configure(configuration).Options!;
    public virtual int GetMemoryKib(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).RamKib;
    public virtual IReadOnlyList<int> GetAudioLatencyChoices(MachineConfiguration configuration) => AudioConstants.LatencyMilliseconds;
    public virtual AudioConfiguration GetAudioConfiguration(MachineConfiguration configuration) => configuration.Audio ?? new AudioConfiguration();
    public virtual MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi) =>
        GraphicsSettingsFunctions.Configure(configuration with { GraphicsApi = graphicsApi }, CoreDefinition.Options);
    public virtual IReadOnlyList<ControllerType> GetControllerTypes(MachineConfiguration configuration) => CoreDefinition.ControllerTypes;
    public virtual IReadOnlyList<ControllerType> GetControllerTypes(MachineConfiguration configuration, int port) => GetControllerTypes(configuration);
    public virtual IReadOnlyList<ControllerType> GetUsbControllerTypes(MachineConfiguration configuration, int port) => [ControllerType.None];
    public virtual int GetControllerPortCount(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).ControllerPortCount;
    public virtual string? GetControllerPortDisplayName(MachineConfiguration configuration, int port) => null;
    public virtual int? GetControllerPortDisplayNumber(MachineConfiguration configuration, int port) => null;
    public IReadOnlyList<EmulationControllerOutputDefinition> GetControllerOutputs(MachineConfiguration configuration, ControllerType type) =>
        CoreDefinition.FeedbackControllerTypes?.Contains(type) == true ? InputSettingsFunctions.RumbleOutputs : [];
    public IReadOnlyList<string> SupportedContentExtensions => CoreDefinition.Extensions;
    public virtual bool IsCpuClock(string fieldId) => CoreSettingsConstants.IsCpuClock(fieldId);
    public virtual int GetMemoryCardCount(MachineConfiguration configuration) => configuration.Model is ModelConstants.PlayStation or ModelConstants.PlayStation2 ? MemoryCardFunctions.Count(configuration) : 0;
    public virtual bool HasFixedMemoryCardDirectories => false;
    public virtual string? GetMemoryCardDirectory(int index) => null;
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
        var selected = EmulationCoreReleaseFunctions.SelectRelease(_availableReleases, release);
        return await ReleaseService(context).InstallAsync(selected, progress, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
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
        ProcessCore? core = null;
        IAudioOutput? audio = null;
        var ownershipTransferred = false;
        try
        {
            core = new ProcessCore(context.HostExecutablePath, context.CorePath);
            audio = native.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null;
            var machine = new Machine(machineId, native, core, native.Media ?? [], sessionDirectory, audio, context.SaveDirectoryResolver?.Invoke(configuration),
                supportsSavedStates: CoreDefinition.SupportsSavedStates);
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

    protected virtual MachineConfiguration Configure(MachineConfiguration configuration) => CoreSettingsFunctions.Configure(CoreDefinition, configuration);

    private CoreReleaseService ReleaseService(EmulatorManagementContext context) => new(context.HttpClient, context.CoreDirectory, ReleaseSettings,
        CoreDefinition.Id, CoreDefinition.IsInstallationComplete, CoreDefinition.PrepareInstallation);
}
