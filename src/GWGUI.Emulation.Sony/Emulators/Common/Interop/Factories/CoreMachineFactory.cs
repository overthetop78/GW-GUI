using GWGUI.Emulation.Sony.Emulators.Common.Interop.Dictionaries;
using CommonControllerFunctions = GWGUI.Emulation.Sony.Common.Functions.ControllerFunctions;
using CommonFirmwareInspection = GWGUI.Emulation.Sony.Common.Machines.Common.Functions.FirmwareConfigurationFunctions;
using CommonStorageSettingsFunctions = GWGUI.Emulation.Sony.Common.Machines.Common.Functions.StorageSettingsFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using System.IO;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sony.Emulators.Common.Functions;
using PlayStationFirmware = GWGUI.Emulation.Sony.Common.Machines.PlayStation.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;

internal abstract class CoreMachineFactory : IEmulatorAdapter, IEmulatorConfigurationValidation, IEmulatorSettings, IEmulatorControllerAdapter, IEmulatorControllerFeedbackAdapter, IEmulatorStorageSettings, IEmulatorContentFormats, IEmulatorFirmwareAdapter
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
    public virtual ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => CommonFirmwareInspection.ScanFileAsync(configuration, path, this, cancellationToken);

    /// <inheritdoc/>
    public virtual EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => CommonStorageSettingsFunctions.Describe(configuration);

    /// <inheritdoc/>
    public virtual MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => CommonStorageSettingsFunctions.Apply(configuration, settings);


    /// <inheritdoc/>
    public virtual bool SupportsPointer(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).MouseButtonCount > 0;
    /// <inheritdoc/>
    public virtual bool SupportsPointerCapture(MachineConfiguration configuration) => SupportsPointer(configuration);

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    protected abstract CoreDefinition CoreDefinition { get; }
    protected CoreReleaseSettings ReleaseSettings => CoreDefinition.ReleaseSettings;
    public bool RequiresExternalFirmware(MachineConfiguration configuration) => CoreDefinition.RequiresExternalFirmware;
    public IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) => configuration.Model == ModelConstants.PlayStation ? [PlayStationFirmware.Selected] : CoreDefinition.Firmware;
    public virtual IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => CoreSettingsFunctions.Blocks(CoreDefinition, configuration);
    /// <summary>Applique les choix de multitap aux options préparées et conserve les autres données de configuration.</summary>
    /// <remarks>Les clés temporaires de ports sont remplacées par la valeur native combinée ; les options empruntées restent inchangées.</remarks>
    public virtual MachineConfiguration ApplySettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var (multitap, mode) in ControllerOptionConstants.MultitapModes)
        {
            if (!values.ContainsKey(multitap + ".port1") && !values.ContainsKey(multitap + ".port2")) continue;
            var previous = options.GetValueOrDefault(multitap, mode.Disabled);
            bool Enabled(int port) => values.TryGetValue(multitap + ".port" + port, out var selected)
                ? selected == EmulationSettingConstants.Enabled : previous == mode.Both || previous == (port == 1 ? mode.Port1 : mode.Port2);
            options[multitap] = (Enabled(1), Enabled(2)) switch
            {
                (true, true) => mode.Both, (true, false) => mode.Port1, (false, true) => mode.Port2, _ => mode.Disabled
            };
            options.Remove(multitap + ".port1");
            options.Remove(multitap + ".port2");
        }
        return configuration with { Options = options };
    }
    public virtual void ValidateConfiguration(MachineConfiguration configuration) { }
    public virtual IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => [];
    public virtual IReadOnlyList<InputBindingDefinition>? GetControllerBindingDefinitions(MachineConfiguration configuration, PeripheralCategory type, int port) =>
        type == PeripheralCategory.Keyboard ? ExternalHostCallbacks.SupportedKeyboardKeys.OrderBy(key => (int)key)
            .Select(key => new InputBindingDefinition(key.ToString(), "Emulation.Sony.Keyboard.Key." + key, key.ToString())).ToArray() : null;
    public virtual IReadOnlyList<EmulationSettingsBlock> GetMachineSettingsBlocks(MachineConfiguration configuration, EmulatorManagementContext? context = null)
    {
        var globalStorage = CoreDefinition.Options.Where(option => option.IsGlobalStorageOption).Select(option => option.Key).ToHashSet(StringComparer.Ordinal);
        return DescribeSettings(configuration, context).Select(block => block.Tab == EmulationMachineTab.Storage
            ? block with { Fields = block.Fields.Where(field => globalStorage.Contains(field.Id)).ToArray() }
            : block).Where(block => block.Fields.Count > 0).ToArray();
    }
    public virtual IReadOnlyList<EmulationSettingsField> GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category) =>
        category == EmulationMediaCategory.CompactDiscDrive
            ? CoreSettingsFunctions.Blocks(CoreDefinition.Options.Where(option => !option.IsGlobalStorageOption).ToArray(), configuration).Where(block => block.Tab == EmulationMachineTab.Storage).SelectMany(block => block.Fields).ToArray()
            : [];
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => Configure(configuration).Options!;
    public virtual decimal? GetMemoryKib(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).RamKib;
    public virtual IReadOnlyList<int> GetAudioLatencyChoices(MachineConfiguration configuration) => AudioConstants.LatencyChoicesMilliseconds;
    public virtual AudioConfiguration GetAudioConfiguration(MachineConfiguration configuration) => configuration.Audio ?? new AudioConfiguration();
    public virtual MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi) =>
        GraphicsSettingsFunctions.Configure(configuration with { GraphicsApi = graphicsApi }, CoreDefinition.Options);
    public virtual IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration) => CoreDefinition.ControllerTypes;
    public virtual IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration, int port) => GetPeripherals(configuration);
    public virtual IReadOnlyList<PeripheralCategory> GetUsbPeripherals(MachineConfiguration configuration, int port) => [PeripheralCategory.None];
    public virtual int GetControllerPortCount(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).ControllerPortCount;
    public virtual string? GetControllerPortDisplayName(MachineConfiguration configuration, int port) => null;
    public virtual int? GetControllerPortDisplayNumber(MachineConfiguration configuration, int port) => null;
    public IReadOnlyList<EmulationControllerOutputDefinition> GetControllerOutputs(MachineConfiguration configuration, PeripheralCategory peripheral) =>
        CoreDefinition.FeedbackControllerTypes?.Contains(peripheral) == true ? CommonControllerFunctions.Feedback.DescribeOutputs(peripheral) : [];
    public IReadOnlyList<string> SupportedContentExtensions => CoreDefinition.Extensions;
    public virtual bool IsCpuClock(string fieldId) => CoreSettingsConstants.IsCpuClock(fieldId);
    public virtual int GetMemoryCardCount(MachineConfiguration configuration) => configuration.Model is ModelConstants.PlayStation or ModelConstants.PlayStation2 ? MemoryCardFunctions.Count(configuration) : 0;
    public virtual IEnumerable<int> GetMemoryCardIndexes(MachineConfiguration configuration) => Enumerable.Range(0, GetMemoryCardCount(configuration));
    public virtual string? GetMemoryCardDisplayLabel(int index) => null;
    public virtual string? MemoryCardInsertedOptionKey => null;
    public virtual bool HasFixedMemoryCardDirectories => false;
    public virtual string? GetMemoryCardDirectory(int index) => null;
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
        CoreHost.Run(pipeName, videoMapName, static (configuration, corePath) => new ExternalCore(CoreCatalog.Get(configuration.EmulatorId), corePath));
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
        await new CoreProvider(ReleaseService(context), path => CoreReleaseService.VerifyWindowsX64Library(path, CoreDefinition.Id)).FindInstalledPathAsync(Emulator, cancellationToken).ConfigureAwait(false);

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
            var machine = new Machine(machineId, native, core, native.Media ?? [], sessionDirectory, null, saveDirectory,
                supportsSavedStates: CoreDefinition.SupportsSavedStates, inputMapper: this, audioOutputFactory: context.AudioOutputFactory, runtimePolicy: this);
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

    protected virtual MachineConfiguration Configure(MachineConfiguration configuration) => CoreSettingsFunctions.Configure(CoreDefinition, configuration);

    private CoreReleaseService ReleaseService(EmulatorManagementContext context) => new(context.HttpClient, context.CoreDirectory, Emulator, ReleaseSettings,
        CoreDefinition.Id, CoreDefinition.IsInstallationComplete, CoreDefinition.PrepareInstallation);
    /// <inheritdoc/>
    public IReadOnlyList<CoreOption> NormalizeRuntimeOptions(MachineConfiguration configuration, IReadOnlyList<CoreOption> options) => CoreOptionMetadataFunctions.Merge(options, CoreDefinition.ResolveOptions?.Invoke(configuration) ?? CoreDefinition.Options, configuration.GraphicsApi);

    /// <inheritdoc/>
    public TimeSpan GetHostCommandTimeout(MachineConfiguration configuration, byte command, TimeSpan fallback) => CoreTimeoutFunctions.For(CoreDefinition, (HostCommand)command);
}
