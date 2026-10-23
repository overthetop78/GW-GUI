using CommonFirmwareInspection = GWGUI.Emulation.Sharp.Common.Machines.Common.Functions.FirmwareFunctions;
using CommonStorageSettingsFunctions = GWGUI.Emulation.Sharp.Common.Machines.Common.Functions.StorageSettingsFunctions;
using GWGUI.Emulation.Sharp.Emulators.Common.Contracts;
using GWGUI.Emulation.Sharp.Emulators.Common.Dictionaries;
using System.IO;

namespace GWGUI.Emulation.Sharp.Emulators.Common.Factories;
internal abstract class MachineFactory(EmulatorCatalogEntry catalogEntry) : IEmulatorAdapter, IEmulatorMediaCapabilities, IEmulatorCapabilities
{
    /// <inheritdoc/>
    public virtual async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => [await CommonFirmwareInspection.ScanFileAsync(path, configuration.Model, configuration.Core, cancellationToken).ConfigureAwait(false)];

    /// <inheritdoc/>
    public virtual EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => CommonStorageSettingsFunctions.Describe(configuration);

    /// <inheritdoc/>
    public virtual MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => CommonStorageSettingsFunctions.Apply(configuration, settings);


    /// <inheritdoc/>
    public abstract EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration);

    /// <inheritdoc/>
    public abstract IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null);

    /// <inheritdoc/>
    public virtual bool SupportsPointerCapture(MachineConfiguration configuration) => this is IEmulatorCapabilities { SupportsNoContent: true };

    public EmulatorCatalogEntry CatalogEntry { get; } = catalogEntry;
    public Emulator Emulator => CatalogEntry.Emulator;
    public string EmulatorId => CatalogEntry.Id;
    public string EmulatorKey => Emulator.ToString();
    public EmulationEmulatorDefinition Definition => CoreCatalog.GetDefinition(CatalogEntry);

    public abstract bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode);
    public abstract ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken);
    public abstract ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken);
    public abstract ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken);
    public abstract ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken);
    protected abstract IEmulatorCore CreateCore(MachineConfiguration configuration, EmulatorCreationContext context);
    bool IEmulatorCapabilities.SupportsNoContent => SupportsNoContent;

    public virtual bool SupportsPointer(MachineConfiguration configuration) => false;
    protected virtual bool RequiresResetRestart => false;
    protected virtual bool SupportsNoContent => false;

    public abstract IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
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