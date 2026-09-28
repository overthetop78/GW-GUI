using GWGUI.Emulation.Sega.Emulators.Flycast.Functions;
using GWGUI.Emulation.Sega.Emulators.Flycast.Contracts;
using GWGUI.Emulation.Sega.Emulators.Flycast.Constants;
using GWGUI.Emulation.Sega.Emulators.Flycast.Services;
using System.IO;

namespace GWGUI.Emulation.Sega.Emulators.Flycast.Factories;

internal sealed class FlycastMachineFactory : IEmulatorAdapter
{
    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public string EmulatorId => FlycastConstants.Id;
    public string EmulatorKey => FlycastConstants.Id;
    public EmulationEmulatorDefinition Definition { get; } = new(
        FlycastConstants.Id, FlycastConstants.DisplayName,
        FlycastConstants.DescriptionResourceKey,
        new[] { "Dreamcast" }.ToHashSet(StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is not [FlycastConstants.CoreHostCommand, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName);
        return true;
    }

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var service = new CoreReleaseService(context.HttpClient, context.CoreDirectory);
        var version = service.GetInstalledVersion();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition, version));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory)
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
        return await new CoreReleaseService(context.HttpClient, context.CoreDirectory)
            .InstallAsync(selected, progress, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken) => new(new CoreProvider(context.HttpClient,
            context.CoreDirectory).FindInstalledPathAsync(cancellationToken));

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) =>
        EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);

    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var native = FlycastOptionFunctions.ToNative(configuration.EnsureId());
        return new Machine(machineId, native,
            new ProcessCore(context.HostExecutablePath, context.CorePath),
            native.Media ?? [], Path.Combine(context.SessionsDirectory,
                machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
            native.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration));
    }
}
