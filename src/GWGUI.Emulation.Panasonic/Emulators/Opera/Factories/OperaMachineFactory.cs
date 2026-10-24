using CommonFirmwareProcessing = GWGUI.Emulation.Panasonic.Common.Functions.CoreHostFunctions;
using CommonFirmwareInspection = GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions.FirmwareFunctions;
using CommonStorageSettingsFunctions = GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions.StorageSettingsFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Panasonic.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using System.IO;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Constants;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Dictionaries;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Services;
using GWGUI.Emulation.Panasonic.Common.Services;
using NativeFirmware = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.FirmwareRuntimeFunctions;
using NativeOptions = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.RuntimeOptionFunctions;
using NativeMedia = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.MediaRuntimeFunctions;
using NativeSettings = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Factories;
internal sealed class OperaMachineFactory : IEmulatorAdapter, IEmulatorSettings, IEmulatorMediaCapabilities, IEmulatorFirmwareAdapter
{
    /// <inheritdoc/>
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(MachineConfiguration configuration, string directory, CancellationToken cancellationToken)
    {
        var candidates = new List<EmulationFirmwareCandidate>();
        CommonFirmwareProcessing.FirmwareProcessing.ScanRecursive(directory, path => candidates.AddRange(ScanFirmwareFileAsync(configuration, path, cancellationToken).GetAwaiter().GetResult()), cancellationToken);
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(candidates);
    }


    /// <inheritdoc/>
    public void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => NativeFirmware.Prepare(configuration, systemDirectory);


    /// <inheritdoc/>
    public bool SupportsNoContent => true;

    /// <inheritdoc/>
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => CommonFirmwareInspection.ScanFileAsync(configuration, path, cancellationToken);

    /// <inheritdoc/>
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => CommonStorageSettingsFunctions.Describe(configuration);

    /// <inheritdoc/>
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => CommonStorageSettingsFunctions.Apply(configuration, settings);


    /// <inheritdoc/>
    public bool SupportsPointer(MachineConfiguration configuration) => InputSettingsFunctions.SupportsPointerCapture(configuration.Input);
    /// <inheritdoc/>
    public bool SupportsPointerCapture(MachineConfiguration configuration) => SupportsPointer(configuration);

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);
    public string EmulatorId => EmulatorConstants.Id;
    public string EmulatorKey => Emulator.Opera.ToString();
    public EmulationEmulatorDefinition Definition { get; } = new(EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey, CoreCatalog.Entry.Models.Select(model => model.ToString()).ToHashSet(StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = CoreHostConstants.NativeOperationSuccess;
        if (arguments is not [CoreHostConstants.CommandLineArgument, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName, (path, emulator) => emulator == Emulator.Opera ? new ExternalCore(path) : throw new ArgumentOutOfRangeException(nameof(emulator)));
        return true;
    }

    public async ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).GetActiveInstallationAsync(Emulator.Opera, cancellationToken).ConfigureAwait(false);
        return new(Definition, installation is null ? null : Path.GetFileName(installation.VersionDirectory));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).GetAvailableAsync(Emulator.Opera, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, $"{item.DeclaredVersion} · {item.PublishedUtc.LocalDateTime:g}", item.DeclaredVersion)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = CoreReleaseFunctions.Releases.SelectRelease(_availableReleases, release, static selected => selected.Emulator == Emulator.Opera);
        var forwarded = CoreReleaseFunctions.Releases.CreateInstallProgress<CoreInstallProgress>(progress, static value => value.Fraction);
        return (await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).InstallAsync(selected, forwarded, cancellationToken).ConfigureAwait(false)).LibraryPath;
    }

    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken) => (await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get).GetActiveInstallationAsync(Emulator.Opera, cancellationToken).ConfigureAwait(false))?.LibraryPath;
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => NativeMedia.Resolve(configuration);
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices;
    public IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration) => null;
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
    public IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) => FirmwareCatalog.Requirements(configuration.Model);
    public bool IsFirmwareCompatible(MachineConfiguration configuration, string fieldId, string path) => Enum.GetValues<FirmwareCategory>().Any(role => CommonFirmwareInspection.Field(role) == fieldId && NativeFirmware.IsCompatible(configuration, role, path));
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => NativeSettings.Create(configuration);
    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ConfigurationFunctions.Validate(configuration);
        if (configuration.Core != Emulator.Opera || !CoreCatalog.Entry.Models.Contains(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();
        IEmulatorCore CreateCore() => OperatingSystem.IsWindows() ? new ProcessCore(context.HostExecutablePath, context.CorePath, Emulator.Opera) : throw new PlatformNotSupportedException();
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
                coreFactory: CreateCore,
                supportsNoContent: ((IEmulatorCapabilities)this).SupportsNoContent, inputMapper: this, runtimePolicy: this);
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
    public bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => slot == EmulationMediaSlot.Cd0;
}