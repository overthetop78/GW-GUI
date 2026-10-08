using GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;
using ReleaseConstants = GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Constants.CoreReleaseConstants;
using EmulatorConstants = GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Constants.EmulatorConstants;
using System.IO;

namespace GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Factories;

internal sealed class CrocoDSMachineFactory : IEmulatorAdapter
{
    private static readonly CoreDefinition Core = new(EmulatorConstants.Id,
        EmulatorConstants.LibraryName, ReleaseConstants.LibraryFile, EmulatorConstants.CoreHostCommand,
        new(ReleaseConstants.OfficialPackageUrl), ReleaseConstants.LatestReleaseDisplayName,
        ReleaseConstants.ReleaseProviderSuffix, ReleaseConstants.Architecture,
        CoreLibraryVerifier.VerifyWindowsX64Library);

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration,
        IReadOnlyList<CoreOption> coreOptions) =>
        GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Functions.CoreSettingsDescriptionFunctions.Create(configuration, coreOptions);
    public string EmulatorId => EmulatorConstants.Id;
    public string EmulatorKey => Emulator.CrocoDS.ToString();
    public IReadOnlyList<CoreOption> GetOptions(EmulatorManagementContext context) =>
        CoreOptionsReader.Read(Path.Combine(context.CoreDirectory, Core.LibraryFile));
    public EmulationEmulatorDefinition Definition { get; } = new(
        EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey,
        new HashSet<string>([EmulatorConstants.MachineId], StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = EmulationModuleConstants.DefaultHostExitCode;
        if (arguments is not [var command, var pipe, var video]
            || command != Core.HostCommand) return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipe, video, path => new ExternalCore(path, Core));
        return true;
    }

    private static CoreReleaseService Releases(EmulatorManagementContext context) =>
        new(context.HttpClient, context.CoreDirectory, Core);

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition,
            Releases(context).GetInstalledVersion()));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await Releases(context).GetAvailableAsync(cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName,
            item.Id, item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context,
        EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        if (!_availableReleases.TryGetValue(release.Id, out var selected))
            throw new ArgumentException(nameof(release));
        return await Releases(context).InstallAsync(selected, progress, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken) => new(new CoreProvider(context.HttpClient,
            context.CoreDirectory, Core).FindInstalledPathAsync(cancellationToken));

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Model != EmulatorConstants.MachineId)
            throw new ArgumentOutOfRangeException(nameof(configuration.Model));
        var media = configuration.Media ?? [];
        if (media.Count(item => item.IsInserted) > EmulatorConstants.MaximumInsertedMediaCount)
            throw new ArgumentOutOfRangeException(nameof(configuration.Media));
        foreach (var item in media)
            if (item.Category is not MediaCategory.Floppy and not MediaCategory.Snapshot
                || !EmulatorConstants.Extensions.Contains(Path.GetExtension(item.Path)))
                throw new ArgumentOutOfRangeException(nameof(configuration.Media));
    }

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration)
    {
        Validate(configuration);
        return EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);
    }

    public Machine Create(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        Validate(configuration);
        var id = Guid.NewGuid();
        // Keep only this core's native options; host/device settings are handled by the module.
        var native = configuration.EnsureId() with
        {
            Options = (configuration.Options ?? new Dictionary<string, string>())
                .Where(item => item.Key.StartsWith(EmulatorConstants.OptionKeyPrefix, StringComparison.Ordinal))
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal)
        };
        return new Machine(id, native, new ProcessCore(context.HostExecutablePath, Core.HostCommand, context.CorePath, supportsPointerCapture: false),
            native.Media ?? [], Path.Combine(context.SessionsDirectory,
                id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
            native.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration));
    }
}
