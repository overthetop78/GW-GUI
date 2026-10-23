using GWGUI.Emulation.Sinclair.Emulators.Common.Dictionaries;
using GWGUI.Emulation.Sinclair.Emulators.Common.Contracts;
using System.IO;

namespace GWGUI.Emulation.Sinclair.Emulators.Common.Factories;
internal abstract class MachineFactory(EmulatorCatalogEntry catalogEntry) : IEmulatorAdapter, IEmulatorMediaCapabilities, IEmulatorInitialContentSettings, IEmulatorCapabilities, IEmulatorControllerAdapter
{
    /// <inheritdoc/>
    public virtual bool SupportsPointerCapture(MachineConfiguration configuration) => configuration.Core != Emulator.None && this is IEmulatorCapabilities { SupportsNoContent: true };

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
    bool IEmulatorInitialContentSettings.SupportsInitialContent => SupportsInitialContent;
    protected virtual bool SupportsInitialContent => false;
    public abstract IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model);
    /// <summary>Projette les sources physiques vers les entrées du cœur par le même contrat que la description des contrôleurs.</summary>
    public abstract EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration);
    public virtual bool SupportsPointer(MachineConfiguration configuration) => false;
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
                supportsPointerCapture: SupportsPointer(configuration),
                requiresResetRestart: RequiresResetRestart,
                supportsNoContent: SupportsNoContent,
                inputMapper: this as IEmulatorInputMapper
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
