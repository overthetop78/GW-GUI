using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Emulators.Libretro.Factories;

internal abstract class MachineFactory(EmulatorCatalogEntry catalogEntry) : IEmulatorAdapter, IEmulatorMediaAdapter
{
    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public EmulatorCatalogEntry CatalogEntry { get; } = catalogEntry;
    public Emulator Emulator => CatalogEntry.Emulator;
    public string EmulatorId => CatalogEntry.Id;
    public string EmulatorKey => Emulator.ToString();
    public EmulationEmulatorDefinition Definition => CoreCatalog.GetDefinition(CatalogEntry);
    internal virtual IReadOnlySet<string>? CartridgeExtensions => null;
    internal virtual bool SupportsCartridgeRegion => CartridgeConstants.SupportsRegionByDefault;
    internal bool SupportsCartridge => CartridgeExtensions is not null;
    internal virtual IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration) =>
        CartridgeExtensions;

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is [CoreHostConstants.CommandLineArgument, var pipeName, var videoMapName])
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            CoreHost.Run(pipeName, videoMapName);
            return true;
        }
        if (arguments is not [CoreOptionProbeConstants.CommandLineArgument, var corePath, var emulatorText]
            || !Enum.TryParse<Emulator>(emulatorText, out var selected) || selected != Emulator) return false;
        try
        {
            Console.Out.WriteLine(CoreOptionProbe.Inspect(corePath, Emulator).Count);
            exitCode = CoreOptionProbeConstants.SuccessExitCode;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(CoreOptionProbe.DescribeFailure(error));
            exitCode = CoreOptionProbeConstants.FailureExitCode;
        }
        return true;
    }

    public async ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory)
            .GetActiveInstallationAsync(Emulator, cancellationToken).ConfigureAwait(false);
        var version = installation is null ? null : Path.GetFileName(installation.VersionDirectory);
        return new EmulationEmulatorInstallation(Definition, version);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory)
            .GetAvailableAsync(Emulator, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id,
            $"{item.DeclaredVersion} · {item.PublishedUtc.LocalDateTime:g}", item.DeclaredVersion)).ToArray();
    }

    public async ValueTask<string> InstallAsync(EmulatorManagementContext context,
        EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        if (!_availableReleases.TryGetValue(release.Id, out var selected) || selected.Emulator != Emulator)
            throw new ArgumentException(nameof(release));
        var adapter = progress is null ? null : new Progress<CoreInstallProgress>(value =>
            progress.Report(value.Fraction ?? 0));
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory)
            .InstallAsync(selected, adapter, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken) =>
        await new CoreProvider(context.HttpClient, context.CoreDirectory)
            .FindInstalledPathAsync(Emulator, cancellationToken).ConfigureAwait(false);

    public IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration) =>
        configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon)
            .OfType<EmulationMedia>().ToArray();

    public virtual MediaConfiguration? SelectPrimaryMedia(MachineConfiguration configuration) =>
        configuration.Media.Where(item => item.IsInserted)
            .OrderBy(item => item.MountOrder)
            .ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

    public virtual IReadOnlyDictionary<string, string> GetConfiguredOptions(MachineConfiguration configuration) =>
        configuration.Options;

    public virtual IReadOnlyDictionary<string, string> PrepareOptions(
        IReadOnlyDictionary<string, string> options) => options;

    public virtual EmulatorPreparedContent? PrepareContent(MachineConfiguration configuration,
        MediaConfiguration? media, string sessionDirectory, ExternalCoreInfo coreInfo)
    {
        if (media is null) return null;
        if (CartridgeExtensions is { } cartridgeExtensions)
        {
            var cartridge = CartridgeFunctions.Prepare(configuration, media, Emulator, cartridgeExtensions,
                coreInfo.NeedsFullPath, coreInfo.Extensions);
            CartridgeFunctions.ValidateNoUnsupportedMetadata(media);
            return new EmulatorPreparedContent(media, cartridge.RuntimePath, cartridge.NeedsFullPath,
                CartridgeFunctions.ApplyOptions(configuration.Options, cartridge.Configuration,
                    SupportsCartridgeRegion),
                State: cartridge);
        }
        var prepared = SessionMediaFunctions.Prepare(media, sessionDirectory, coreInfo.Extensions);
        return new EmulatorPreparedContent(media, prepared.RuntimePath, coreInfo.NeedsFullPath,
            configuration.Options, prepared);
    }

    public virtual SessionMedia? PrepareInsertedMedia(MachineConfiguration configuration,
        MediaConfiguration media, string sessionDirectory, ExternalCoreInfo coreInfo) => null;

    public virtual void ValidatePreparedContent(EmulatorPreparedContent? preparedContent,
        bool diskControlAvailable) { }

    public virtual void ValidateInsertion(MachineConfiguration configuration, MediaConfiguration media) { }

    public virtual bool SupportsDiskControlOperations => false;

    public virtual bool SupportsDiskControl(MediaConfiguration media) => false;

    public virtual bool SupportsEjection(EmulationMediaSlot slot) => !SupportsCartridge;

    public virtual Exception ContentLoadException(string message) =>
        new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, message);

    public virtual void CleanupPreparedContent(EmulatorPreparedContent? preparedContent) { }

    public Machine Create(MachineConfiguration configuration,
        EmulatorCreationContext context)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();
        if (configuration.Core != Emulator)
            throw new ArgumentException(nameof(configuration));
        var machineId = Guid.NewGuid();
        var core = new ProcessCore(context.HostExecutablePath, context.CorePath, Emulator);
        return new Machine(machineId, configuration, core,
            Path.Combine(context.SessionsDirectory,
                machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
            audioOutputFactory: configuration.AudioEnabled ? context.AudioOutputFactory : null,
            saveDirectory: context.SaveDirectoryResolver?.Invoke(configuration));
    }
}
