using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;
using CommonStorageSettingsFunctions = GWGUI.Emulation.Snk.Common.Machines.Common.Functions.StorageSettingsFunctions;
using GWGUI.Emulation.Snk.Emulators.Common.Contracts;
using System.IO;

namespace GWGUI.Emulation.Snk.Emulators.Common.Factories;
internal abstract class MachineFactory(EmulatorCatalogEntry catalogEntry) : IEmulatorAdapter, IEmulatorSettingsAdapter, IEmulatorFirmwareAdapter, IEmulatorFirmwareSettings, IEmulatorInitialContentSettings, IEmulatorMediaCapabilities, IEmulatorControllerAdapter, IEmulatorPointerCapabilities
{
    /// <inheritdoc/>
    public virtual MachineConfiguration SelectConfiguration(MachineConfiguration configuration)
    {
        var core = configuration.Core;
        var options = CommonCoreHostFunctions.OptionProcessing.FilterForEmulator(CoreOptionValidationFunctions.ForCore(core).Select(option => option.Key), configuration.Options, SettingsConstants.AudioOutput, SettingsConstants.AudioLatency, VideoAudioSettingsConstants.AudioVolumeOption);
        var controllers = configuration.Input.Controllers?.Select(binding => InputSettingsFunctions.Peripherals(core).Contains(binding.Peripheral) ? binding : binding with { Peripheral = InputSettingsFunctions.DefaultPeripheral(configuration.Model) }).ToArray();
        var result = configuration with
        {
            Core = core,
            Options = options,
            Input = configuration.Input with
            {
                Controllers = controllers
            }
        };
        var devices = DescribeMediaDevices(result);
        var slots = devices.Select(device => device.Slot).ToHashSet();
        if (result.Media.Any(media => !slots.Contains(media.Slot)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, "emulatorId");
        result = result with { ConfiguredSlots = result.ConfiguredSlots.Where(slots.Contains).ToArray() };
        return result;
    }

    /// <inheritdoc/>
    public virtual EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => CommonStorageSettingsFunctions.Describe(configuration);

    /// <inheritdoc/>
    public virtual MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => CommonStorageSettingsFunctions.Apply(configuration, settings);


    /// <inheritdoc/>
    public abstract EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration);

    /// <inheritdoc/>
    public virtual bool SupportsPointerCapture(MachineConfiguration configuration) => false;

    public virtual int GetControllerPortCount(MachineConfiguration configuration) => ConfigurationFunctions.ControllerPortCount(configuration.Model);
    public abstract IReadOnlyList<CoreOption> CoreOptions { get; }

    public abstract IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null);
    public virtual bool IsDeferredOption(MachineConfiguration configuration, string key) => false;
    public abstract IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model);
    public abstract bool IsFirmwareRequired(MachineModel model, FirmwareCategory category);
    public abstract ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken);
    public EmulatorCatalogEntry CatalogEntry { get; } = catalogEntry;
    public Emulator Emulator => CatalogEntry.Emulator;
    public string EmulatorId => CatalogEntry.Id;
    public string EmulatorKey => Emulator.ToString();
    public EmulationEmulatorDefinition Definition => new(CatalogEntry.Id, CatalogEntry.LibraryName, CatalogEntry.DescriptionResourceKey ?? $"Emulation.Emulator.{CatalogEntry.Id}.Description", CatalogEntry.Models.Select(model => model.ToString()).ToHashSet(StringComparer.Ordinal));

    public abstract bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode);
    public abstract ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken);
    public abstract ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken);
    public abstract ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken);
    public abstract ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken);
    protected abstract IEmulatorCore CreateCore(MachineConfiguration configuration, EmulatorCreationContext context);
    public virtual bool SupportsPointer(MachineConfiguration configuration) => false;
    protected virtual bool RequiresResetRestart => false;
    protected virtual bool SupportsNoContent => false;

    public abstract IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
    public virtual IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => configuration.InitialContent is { Length: > 0 } path ? new[]
    {
        path
    }

    : Array.Empty<string>();
    public abstract bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path);
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray();
    public virtual IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options;
    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ConfigurationFunctions.Validate(configuration);
        if (configuration.Core != Emulator || !CatalogEntry.Models.Contains(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        var id = Guid.NewGuid();
        var core = CreateCore(configuration, context);
        var completed = false;
        try
        {
            var machine = new Machine(
                id,
                configuration,
                core,
                Path.Combine(context.SessionsDirectory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
                audioOutputFactory: configuration.AudioEnabled ? context.AudioOutputFactory : null,
                saveDirectory: context.SaveDirectoryResolver?.Invoke(configuration),
                coreFactory: () => CreateCore(configuration, context),
                supportsPointerCapture: SupportsPointer(configuration),
                requiresResetRestart: RequiresResetRestart,
                supportsNoContent: SupportsNoContent,
                inputMapper: this, runtimePolicy: this
            );
            completed = true;
            return machine;
        }
        finally
        {
            if (!completed)
                core.Dispose();
        }
    }
}