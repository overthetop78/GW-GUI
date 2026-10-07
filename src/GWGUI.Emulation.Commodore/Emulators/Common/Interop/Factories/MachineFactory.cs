using System.IO;

using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Factories;

internal abstract class MachineFactory : IEmulatorAdapter
{
    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public abstract EmulatorCatalogEntry CatalogEntry { get; }
    internal abstract CoreDefinition CoreDefinition { get; }
    internal virtual MachineConfiguration PrepareConfiguration(MachineConfiguration configuration) => configuration;
    internal abstract IReadOnlyDictionary<string, string> NativeOptions(MachineConfiguration configuration, IReadOnlyList<MediaConfiguration> media);
    internal virtual string? KickstartOverrideOption => null;
    internal virtual string ExtendedRomFileName(string model, string path) => ExternalCore.ResolveExtendedRomFileName(model, path);
    internal virtual string? PrepareContent(MachineConfiguration configuration, string session, IReadOnlyList<MediaConfiguration> media) => ExternalCore.PrepareContentPath(configuration, session, media);
    public Emulator Emulator => CatalogEntry.Emulator;
    public string EmulatorId => CatalogEntry.Definition.Id;
    public string EmulatorKey => Emulator.ToString();
    public EmulationEmulatorDefinition Definition => CatalogEntry.Definition;

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is not [ProcessCoreConstants.CoreHost, var pipeName, var videoMapName])
            return false;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        CoreHost.Run(pipeName, videoMapName);
        return true;
    }

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var version = new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreDefinition)
            .GetInstalledVersion();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition, version));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreDefinition)
            .GetAvailableAsync(cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName,
            item.Id, item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context,
        EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        if (!_availableReleases.TryGetValue(release.Id, out var selected))
            throw new ArgumentException(nameof(release));
        return await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreDefinition)
            .InstallAsync(selected, progress, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken) =>
        await new CoreProvider(context.HttpClient, context.CoreDirectory, CoreDefinition)
            .FindInstalledPathAsync(cancellationToken).ConfigureAwait(false);

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) =>
        EmulationMediaConversionFunctions.ToCommon(ExternalCore.ResolveConfiguredMedia(configuration));

    public Machine Create(MachineConfiguration configuration,
        EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var core = new ProcessCore(context.HostExecutablePath, context.CorePath);
        var configured = PrepareConfiguration(configuration.EnsureId());
        return new Machine(machineId, configured, core,
            ExternalCore.ResolveConfiguredMedia(configured),
            Path.Combine(context.SessionsDirectory,
                machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
            configuration.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration),
            startErrorTranslator: error => EmulationErrorService.Translate(error,
                EmulationMessageCategory.Machine,
                EmulationMessageCode.MachineStartFailed,
                new EmulationMachineMessageContext(configuration.Model)));
    }
}
