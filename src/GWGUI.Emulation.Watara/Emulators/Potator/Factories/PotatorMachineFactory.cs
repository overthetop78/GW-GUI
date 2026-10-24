using CommonStorageSettingsFunctions = GWGUI.Emulation.Watara.Common.Machines.Common.Functions.StorageSettingsFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Watara.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using System.IO;
using GWGUI.Emulation.Watara.Emulators.Potator.Constants;
using GWGUI.Emulation.Watara.Emulators.Potator.Dictionaries;
using GWGUI.Emulation.Watara.Emulators.Potator.Interop.Services;
using GWGUI.Emulation.Watara.Common.Services;
using NativeMedia = GWGUI.Emulation.Watara.Emulators.Potator.Functions.MediaRuntimeFunctions;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Factories;
internal sealed class PotatorMachineFactory : IEmulatorAdapter, IEmulatorMediaCapabilities, IEmulatorSettings
{
    /// <inheritdoc/>
    public IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);

    /// <inheritdoc/>
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([]);
    }

    /// <inheritdoc/>
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => CommonStorageSettingsFunctions.Describe(configuration);

    /// <inheritdoc/>
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => CommonStorageSettingsFunctions.Apply(configuration, settings);


    /// <inheritdoc/>
    public bool SupportsPointerCapture(MachineConfiguration configuration) => false;

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);
    public string EmulatorId => EmulatorConstants.Id;
    public string EmulatorKey => Emulator.Potator.ToString();
    public EmulationEmulatorDefinition Definition { get; } = new(EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey, CoreCatalog.Entry.Models.Select(model => model.ToString()).ToHashSet(StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = CoreHostConstants.NativeOperationSuccess;
        if (arguments is not [CoreHostConstants.CommandLineArgument, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName, (path, emulator) => emulator == Emulator.Potator ? new ExternalCore(path) : throw new ArgumentOutOfRangeException(nameof(emulator)));
        return true;
    }

    public async ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).GetActiveInstallationAsync(Emulator.Potator, cancellationToken).ConfigureAwait(false);
        return new EmulationEmulatorInstallation(Definition, installation is null ? null : Path.GetFileName(installation.VersionDirectory));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).GetAvailableAsync(Emulator.Potator, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, $"{item.DeclaredVersion} · {item.PublishedUtc?.LocalDateTime:g}", item.DeclaredVersion)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = CoreReleaseFunctions.Releases.SelectRelease(_availableReleases, release, static selected => selected.Emulator == Emulator.Potator);
        var forwarded = CoreReleaseFunctions.Releases.CreateInstallProgress<CoreInstallProgress>(progress, static value => value.Fraction);
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).InstallAsync(selected, forwarded, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken) => await new CoreProvider(new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get)).FindInstalledPathAsync(Emulator.Potator, cancellationToken).ConfigureAwait(false);
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray();
    public IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration) => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "sv"
    };
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices(configuration);
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => GWGUI.Emulation.Watara.Emulators.Potator.Functions.SettingsDescriptionFunctions.Create(configuration);
    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ConfigurationFunctions.Validate(configuration);
        if (configuration.Core != Emulator.Potator || !CoreCatalog.Entry.Models.Contains(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();
        IEmulatorCore CreateCore() => OperatingSystem.IsWindows() ? new ProcessCore(context.HostExecutablePath, context.CorePath, Emulator.Potator) : throw new PlatformNotSupportedException();
        var id = Guid.NewGuid();
        var saveDirectory = context.SaveDirectoryResolver?.Invoke(configuration);
        if (saveDirectory is not null) ((IEmulatorPersistentStorage)this).EnsurePersistentStorage(configuration, saveDirectory);
        var core = CreateCore();
        var completed = false;
        try
        {
            var machine = new Machine(
                id,
                configuration,
                core,
                Path.Combine(context.SessionsDirectory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
                audioOutputFactory: configuration.AudioEnabled ? context.AudioOutputFactory : null,
                saveDirectory: saveDirectory,
                coreFactory: CreateCore, inputMapper: this, runtimePolicy: this, supportsNoContent: ((IEmulatorCapabilities)this).SupportsNoContent);
            completed = true;
            return machine;
        }
        finally
        {
            if (!completed)
                core.Dispose();
        }
    }

    /// <inheritdoc/>
    public bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
}