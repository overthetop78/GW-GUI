using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Sega.Common.Contracts;
using GWGUI.Emulation.Sega.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Contracts;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Functions;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Services;
using GWGUI.Emulation.Sega.Emulators.PicoDrive.Constants;
using GWGUI.Emulation.Sega.Emulators.PicoDrive.Services;

namespace GWGUI.Emulation.Sega.Emulators.PicoDrive.Factories;

internal sealed class PicoDriveMachineFactory : IEmulatorAdapter
{
    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public string EmulatorId => PicoDriveConstants.Id;
    public string EmulatorKey => PicoDriveConstants.Id;
    public EmulationEmulatorDefinition Definition { get; } = new(
        PicoDriveConstants.Id, PicoDriveConstants.DisplayName,
        PicoDriveConstants.DescriptionResourceKey,
        new[] { ModelConstants.MasterSystem, ModelConstants.MegaDrive }
            .ToHashSet(StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is not [PicoDriveConstants.CoreHostCommand, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName, PicoDriveConstants.LibraryName, false);
        return true;
    }

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var service = CreateReleaseService(context);
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition,
            service.GetInstalledVersion()));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await CreateReleaseService(context).GetAvailableAsync(cancellationToken)
            .ConfigureAwait(false);
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
        return await CreateReleaseService(context).InstallAsync(selected, progress, cancellationToken)
            .ConfigureAwait(false);
    }

    public ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken) => new(
        new PicoDriveCoreProvider(context.CoreDirectory).FindInstalledPathAsync(cancellationToken));

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) =>
        EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);

    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var native = GenesisPlusGXOptionFunctions.ToNative(configuration.EnsureId());
        return new Machine(machineId, native,
            new ProcessCore(context.HostExecutablePath, context.CorePath,
                PicoDriveConstants.CoreHostCommand),
            native.Media ?? [], Path.Combine(context.SessionsDirectory,
                machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
            native.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration));
    }

    private static CoreReleaseService CreateReleaseService(EmulatorManagementContext context) =>
        new(context.HttpClient, context.CoreDirectory,
            new Uri(PicoDriveConstants.LatestOfficialUri), PicoDriveConstants.LibraryFileName,
            PicoDriveConstants.DisplayName);
}
