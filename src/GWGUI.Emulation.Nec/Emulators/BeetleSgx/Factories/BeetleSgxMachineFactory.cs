using System.IO;
using GWGUI.Emulation.Nec.Common.Machines.SuperGrafx.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Contracts;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Services;
using GWGUI.Emulation.Nec.Emulators.BeetleSgx.Constants;

namespace GWGUI.Emulation.Nec.Emulators.BeetleSgx.Factories;

internal sealed class BeetleSgxMachineFactory : IEmulatorAdapter
{
    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public string EmulatorId => BeetleSgxConstants.Id;
    public string EmulatorKey => BeetleSgxConstants.Id;
    public EmulationEmulatorDefinition Definition { get; } = new(
        BeetleSgxConstants.Id, BeetleSgxConstants.DisplayName,
        BeetleSgxConstants.DescriptionResourceKey,
        new[] { SuperGrafxMachineConstants.Id }.ToHashSet(StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        return false;
    }

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var service = ReleaseService(context);
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition,
            service.GetInstalledVersion()));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await ReleaseService(context).GetAvailableAsync(cancellationToken)
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
        return await ReleaseService(context).InstallAsync(selected, progress, cancellationToken)
            .ConfigureAwait(false);
    }

    public ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ReleaseService(context).RequiredLibraryPath;
        if (!File.Exists(path)) return ValueTask.FromResult<string?>(null);
        try
        {
            CoreReleaseService.VerifyWindowsX64Library(path);
            return ValueTask.FromResult<string?>(path);
        }
        catch (IOException) { return ValueTask.FromResult<string?>(null); }
        catch (InvalidDataException) { return ValueTask.FromResult<string?>(null); }
    }

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) =>
        EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);

    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var native = BeetlePceOptionFunctions.ToNative(configuration.EnsureId());
        return new Machine(machineId, native,
            new ProcessCore(context.HostExecutablePath, context.CorePath),
            native.Media ?? [], Path.Combine(context.SessionsDirectory,
                machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
            native.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration));
    }

    private static CoreReleaseService ReleaseService(EmulatorManagementContext context) =>
        new(context.HttpClient, context.CoreDirectory, BeetleSgxConstants.ReleaseSettings);
}
