using System.IO;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Constants;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Contracts;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Dictionaries;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Services;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Services;
using NativeFirmware = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.FirmwareRuntimeFunctions;
using NativeOptions = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.RuntimeOptionFunctions;
using NativeMedia = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.MediaRuntimeFunctions;
using NativeSettings = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Factories;
internal sealed class OperaMachineFactory : IEmulatorAdapter, IEmulatorFirmwareCapabilities, IEmulatorSettings, IEmulatorMediaCapabilities
{
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
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory).GetActiveInstallationAsync(Emulator.Opera, cancellationToken).ConfigureAwait(false);
        return new(Definition, installation is null ? null : Path.GetFileName(installation.VersionDirectory));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory).GetAvailableAsync(Emulator.Opera, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, $"{item.DeclaredVersion} · {item.PublishedUtc.LocalDateTime:g}", item.DeclaredVersion)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        if (!_availableReleases.TryGetValue(release.Id, out var selected) || selected.Emulator != Emulator.Opera)
            throw new ArgumentException(nameof(release));
        var forwarded = progress is null ? null : new Progress<CoreInstallProgress>(value => progress.Report(value.Fraction ?? 0));
        return (await new CoreReleaseService(context.HttpClient, context.CoreDirectory).InstallAsync(selected, forwarded, cancellationToken).ConfigureAwait(false)).LibraryPath;
    }

    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken) => (await new CoreReleaseService(context.HttpClient, context.CoreDirectory).GetActiveInstallationAsync(Emulator.Opera, cancellationToken).ConfigureAwait(false))?.LibraryPath;
    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) => NativeMedia.Resolve(configuration);
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices;
    public IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration) => null;
    public IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
    public IReadOnlyList<FirmwareRequirement> DescribeFirmware(MachineConfiguration configuration) => FirmwareCatalog.Requirements(configuration.Model);
    public bool IsFirmwareCompatible(MachineConfiguration configuration, FirmwareRole role, string path) => NativeFirmware.IsCompatible(configuration, role, path);
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => NativeSettings.Create(configuration);
    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ConfigurationFunctions.Validate(configuration);
        if (configuration.Core != Emulator.Opera || !CoreCatalog.Entry.Models.Contains(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();
        IEmulatorCore CreateCore() => OperatingSystem.IsWindows() ? new ProcessCore(context.HostExecutablePath, context.CorePath, Emulator.Opera) : throw new PlatformNotSupportedException();
        var id = Guid.NewGuid();
        var core = CreateCore();
        var completed = false;
        try
        {
            var machine = new Machine(
                id,
                configuration,
                core,
                Path.Combine(context.SessionsDirectory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
                (slot, _) => slot == EmulationMediaSlot.Cd0,
                audioOutputFactory: configuration.AudioEnabled ? context.AudioOutputFactory : null,
                saveDirectory: context.SaveDirectoryResolver?.Invoke(configuration),
                coreFactory: CreateCore,
                supportsNoContent: true
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