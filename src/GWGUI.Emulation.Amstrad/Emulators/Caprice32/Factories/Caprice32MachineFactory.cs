using GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;
using ReleaseConstants = GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants.CoreReleaseConstants;
using EmulatorConstants = GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants.EmulatorConstants;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;
using System.IO;

namespace GWGUI.Emulation.Amstrad.Emulators.Caprice32.Factories;

internal sealed class Caprice32MachineFactory : IEmulatorAdapter
{
    private static readonly CoreDefinition Core = new(EmulatorConstants.Id,
        EmulatorConstants.LibraryName, ReleaseConstants.LibraryFile, EmulatorConstants.CoreHostCommand,
        new(ReleaseConstants.OfficialPackageUrl), ReleaseConstants.LatestReleaseDisplayName,
        ReleaseConstants.ReleaseProviderSuffix, ReleaseConstants.Architecture,
        CoreLibraryVerifier.VerifyWindowsX64Library);

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public string EmulatorId => EmulatorConstants.Id;
    public string EmulatorKey => Emulator.Caprice32.ToString();
    public IReadOnlyList<CoreOption> GetOptions(EmulatorManagementContext context) => [];
    public EmulationEmulatorDefinition Definition { get; } = new(
        EmulatorConstants.Id, EmulatorConstants.DisplayName,
        EmulatorConstants.DescriptionResourceKey,
        MachineCatalog.All.Select(machine => machine.Id).ToHashSet(StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = EmulationModuleConstants.DefaultHostExitCode;
        if (arguments is not [EmulatorConstants.CoreHostCommand, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName, path => new ExternalCore(path, Core));
        return true;
    }

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var service = new CoreReleaseService(context.HttpClient, context.CoreDirectory, Core);
        var version = service.GetInstalledVersion();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition, version));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, Core)
            .GetAvailableAsync(cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName,
            item.Id, item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context,
        EmulationEmulatorRelease release, IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        if (!_availableReleases.TryGetValue(release.Id, out var selected))
            throw new ArgumentException(nameof(release));
        return await new CoreReleaseService(context.HttpClient, context.CoreDirectory, Core)
            .InstallAsync(selected, progress, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken) => new(new CoreProvider(context.HttpClient,
            context.CoreDirectory, Core).FindInstalledPathAsync(cancellationToken));

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) =>
        EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);

    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var native = Caprice32OptionFunctions.ToNative(configuration.EnsureId());
        return new Machine(machineId, native,
            new ProcessCore(context.HostExecutablePath, Core.HostCommand, context.CorePath),
            native.Media ?? [], Path.Combine(context.SessionsDirectory,
                machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
            native.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration));
    }
}
