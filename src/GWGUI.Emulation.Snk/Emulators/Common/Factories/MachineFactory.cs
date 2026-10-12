using System.IO;

namespace GWGUI.Emulation.Snk.Emulators.Common.Factories;
internal abstract class MachineFactory(EmulatorCatalogEntry catalogEntry) : IEmulatorAdapter, IEmulatorSettingsAdapter, IEmulatorFirmwareAdapter, IEmulatorMediaAdapter, IEmulatorControllerAdapter
{
    public virtual int ControllerPortCount(MachineConfiguration configuration) => ConfigurationFunctions.ControllerPortCount(configuration.Model);
    public abstract IReadOnlyList<CoreOption> CoreOptions { get; }

    public abstract IReadOnlyList<CoreOption> DescribeOptions(MachineConfiguration configuration);
    public virtual bool IsDeferredOption(MachineConfiguration configuration, string key) => false;
    public abstract IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model);
    public abstract bool IsFirmwareRequired(MachineModel model, FirmwareCategory category);
    public abstract ValueTask<EmulationFirmwareCandidate> InspectFirmwareAsync(string path, MachineModel model, CancellationToken cancellationToken);
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
    protected virtual bool SupportsPointerCapture => false;
    protected virtual bool RequiresResetRestart => false;
    protected virtual bool SupportsNoContent => false;

    public abstract IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
    public virtual IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => configuration.InitialContent is { Length: > 0 } path ? new[]
    {
        path
    }

    : Array.Empty<string>();
    protected abstract bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path);
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray();
    public virtual IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration)
    {
        var devices = DescribeMediaDevices(configuration).Where(device => device.MediaType == EmulationMediaType.Cartridge).ToArray();
        return devices.Length == 0 ? null : devices.SelectMany(device => device.AcceptedExtensions).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

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
                (slot, path) => RequiresMediaRestart(configuration, slot, path),
                audioOutputFactory: configuration.AudioEnabled ? context.AudioOutputFactory : null,
                saveDirectory: context.SaveDirectoryResolver?.Invoke(configuration),
                coreFactory: () => CreateCore(configuration, context),
                supportsPointerCapture: SupportsPointerCapture,
                requiresResetRestart: RequiresResetRestart,
                supportsNoContent: SupportsNoContent
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