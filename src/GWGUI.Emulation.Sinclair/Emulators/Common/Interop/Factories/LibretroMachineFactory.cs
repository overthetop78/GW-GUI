using GWGUI.Emulation.Sinclair.Emulators.Common.Dictionaries;
using CoreReleaseFunctions = GWGUI.Emulation.Sinclair.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sinclair.Emulators.Common.Factories;
using GWGUI.Emulation.Sinclair.Emulators.Common.Contracts;
using System.IO;

namespace GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Factories;

internal abstract class LibretroMachineFactory(EmulatorCatalogEntry catalogEntry) : GWGUI.Emulation.Sinclair.Emulators.Common.Factories.MachineFactory(catalogEntry), IEmulatorFirmwareAdapter
{
    /// <inheritdoc/>
    public override EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputMappingFunctions.Apply(snapshot, configuration.Input, configuration.Model, Emulator);

    public abstract void PrepareFirmware(MachineConfiguration configuration, string systemDirectory);
    internal abstract LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions);
    internal virtual bool SupportsMediaExchange(MachineConfiguration configuration, EmulationMediaSlot slot) => false;
    internal virtual void CompleteMediaLoad(MachineConfiguration configuration, string sessionDirectory, ExternalHostCallbacks callbacks) { }

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases = new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public override bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = CoreHostConstants.NativeOperationSuccess;
        if (arguments is [CoreHostConstants.CommandLineArgument, var pipeName, var videoMapName])
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            CoreHost.Run(pipeName, videoMapName, (path, emulator) => new ExternalCore(path, emulator));
            return true;
        }
        if (arguments is not [CoreOptionProbeConstants.CommandLineArgument, var corePath, var emulatorText] || !Enum.TryParse<Emulator>(emulatorText, out var selected) || selected != Emulator) return false;
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

    public override async ValueTask<EmulationEmulatorInstallation> GetInstallationAsync( EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get) .GetActiveInstallationAsync(Emulator, cancellationToken).ConfigureAwait(false);
        var version = installation is null ? null : Path.GetFileName(installation.VersionDirectory);
        return new EmulationEmulatorInstallation(Definition, version);
    }

    public override async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync( EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get) .GetAvailableAsync(Emulator, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, $"{item.DeclaredVersion} · {item.PublishedUtc.LocalDateTime:g}", item.DeclaredVersion)).ToArray();
    }

    public override async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = CoreReleaseFunctions.Releases.SelectRelease(_availableReleases, release, selected => selected.Emulator == Emulator);
        var adapter = CoreReleaseFunctions.Releases.CreateInstallProgress<CoreInstallProgress>(progress, static value => value.Fraction);
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, CoreCatalog.Get) .InstallAsync(selected, adapter, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public override async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken) => await new CoreProvider(context.HttpClient, context.CoreDirectory, CoreCatalog.Get) .FindInstalledPathAsync(Emulator, cancellationToken).ConfigureAwait(false);

    protected override IEmulatorCore CreateCore(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        return new ProcessCore(context.HostExecutablePath, context.CorePath, Emulator);
    }
}
