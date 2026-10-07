using System.IO;
using System.Security.Cryptography;

using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Factories;

internal abstract class MachineFactory : IEmulatorAdapter
{
    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public abstract EmulatorCatalogEntry CatalogEntry { get; }
    internal abstract CoreDefinition CoreDefinition { get; }
    internal virtual MachineConfiguration PrepareConfiguration(MachineConfiguration configuration) => configuration;
    internal abstract IReadOnlyDictionary<string, string> NativeOptions(MachineConfiguration configuration, IReadOnlyList<MediaConfiguration> media);
    internal virtual string? FirmwareOverrideOption => null;
    internal abstract void PrepareFirmware(MachineConfiguration configuration, string systemDirectory);
    internal abstract string? PrepareContent(MachineConfiguration configuration, string session, IReadOnlyList<MediaConfiguration> media);
    internal virtual string ConvertMediaPath(string path, string conversionDirectory) => path;
    internal virtual int ControllerPortCount(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).ControllerPortCount;
    internal virtual uint ControllerDevice(IReadOnlyList<IReadOnlyList<ControllerDevice>> ports, int port, ControllerType type) => ExternalCore.ControllerDevice(ports, port, type);
    internal virtual MediaCategory InferMediaCategory(MachineConfiguration configuration, string path)
    {
        var extension = Path.GetExtension(path);
        var device = GetMediaDevices(configuration).FirstOrDefault(device =>
            device.AcceptedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
        return device?.MediaType switch
        {
            EmulationMediaType.Floppy => MediaCategory.Floppy,
            EmulationMediaType.HardDisk => MediaCategory.HardDrive,
            EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
            EmulationMediaType.Cartridge => MediaCategory.Cartridge,
            EmulationMediaType.Cassette => MediaCategory.Cassette,
            _ => throw new InvalidDataException(CoreExceptions.UnsupportedContentExtension(extension))
        };
    }
    public abstract IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);
    public virtual IReadOnlyList<EmulationFirmwareCandidate> ScanFirmware(MachineConfiguration configuration, string directory, CancellationToken cancellationToken)
    {
        var slots = GetFirmwareSlots(configuration);
        return Directory.EnumerateFiles(directory, MachineSettingsConstants.AllFirmwareFiles, SearchOption.AllDirectories)
            .SelectMany(path =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var stream = File.OpenRead(path);
                var sha256 = Convert.ToHexString(SHA256.HashData(stream));
                return slots.Select(slot => new EmulationFirmwareCandidate(
                    sha256 + MachineSettingsConstants.CandidateRoleSeparator + slot.FieldId, path,
                    Path.GetFileName(path), null, EmulationFirmwareCompatibility.PartiallyCompatible, slot.FieldId));
            }).ToArray();
    }
    public virtual ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory) => ValueTask.FromResult(media);
    public virtual Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory) => Task.FromResult(configuration);
    public abstract IReadOnlyList<EmulationMediaDevice> GetMediaDevices(MachineConfiguration configuration);
    public virtual IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(Model model, MachineConfiguration configuration) =>
        SettingsDescriptionFunctions.CreateBasic(model, configuration);
    public virtual EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) =>
        StorageSettingsFunctions.DescribeBasic(configuration);
    public virtual MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) =>
        StorageSettingsFunctions.ApplyBasic(configuration, settings);
    public virtual MachineConfiguration ApplyBackendSettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values) => configuration;
    public virtual EmulationConfigurationSummary Summarize(MachineConfiguration configuration) =>
        ConfigurationSummaryFunctions.CreateBasic(configuration);
    public virtual void ValidateConfiguration(MachineConfiguration configuration) { }
    public virtual IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration) =>
        InputSettingsFunctions.GenericKeyboardDefinitions();
    public int GetControllerPortCount(MachineConfiguration configuration) => ControllerPortCount(configuration);
    public virtual MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration) => configuration;
    public virtual IReadOnlySet<EmulationKey> SoftResetKeys { get; } = new HashSet<EmulationKey>();
    public Emulator Emulator => CatalogEntry.Emulator;
    public string EmulatorId => CatalogEntry.Definition.Id;
    public string EmulatorKey => Emulator.ToString();
    public EmulationEmulatorDefinition Definition => CatalogEntry.Definition;

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = CoreHostConstants.SuccessfulExitCode;
        if (arguments is not [ProcessCoreConstants.CoreHost, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName);
        return true;
    }

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var version = new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreDefinition)
            .GetInstalledVersion();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition, version));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreDefinition)
            .GetAvailableAsync(cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName,
            item.Id, item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context,
        EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        if (!_availableReleases.TryGetValue(release.Id, out var selected))
            throw new ArgumentException(nameof(release));
        return await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreDefinition)
            .InstallAsync(selected, progress, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken) =>
        await new CoreProvider(context.HttpClient, context.CoreDirectory, CoreDefinition)
            .FindInstalledPathAsync(cancellationToken).ConfigureAwait(false);

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) =>
        EmulationMediaConversionFunctions.ToCommon(ExternalCore.ResolveConfiguredMedia(configuration));

    public virtual IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates) =>
        new Dictionary<EmulationMediaSlot, bool>();

    public Machine Create(MachineConfiguration configuration,
        EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var configured = PrepareConfiguration(configuration.EnsureId());
        var core = new ProcessCore(context.HostExecutablePath, context.CorePath);
        IAudioOutput? audio = null;
        var ownershipTransferred = false;
        try
        {
            audio = configured.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null;
            var machine = new Machine(machineId, configured, core,
                ExternalCore.ResolveConfiguredMedia(configured),
                Path.Combine(context.SessionsDirectory,
                    machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
                audio, context.SaveDirectoryResolver?.Invoke(configured),
                startErrorTranslator: error => EmulationErrorService.Translate(error,
                    EmulationMessageCategory.Machine,
                    EmulationMessageCode.MachineStartFailed,
                    new EmulationMachineMessageContext(configured.Model)));
            ownershipTransferred = true;
            return machine;
        }
        finally
        {
            if (!ownershipTransferred)
            {
                try { core.Dispose(); }
                finally { audio?.Dispose(); }
            }
        }
    }
}
