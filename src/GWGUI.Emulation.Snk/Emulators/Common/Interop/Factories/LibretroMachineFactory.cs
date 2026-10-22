using CoreReleaseFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Snk.Emulators.Common.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Contracts;
using System.IO;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;

internal abstract class LibretroMachineFactory(EmulatorCatalogEntry catalogEntry) : GWGUI.Emulation.Snk.Emulators.Common.Factories.MachineFactory(catalogEntry), IEmulatorInputMapper
{
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputMappingFunctions.Apply(snapshot, configuration, ControllerPortCount(configuration));

    internal virtual int DirectoryStringBufferCapacity => 0;
    internal virtual string ExpectedNativeLibraryName => CatalogEntry.LibraryName;
    internal virtual IReadOnlyDictionary<ControllerCommand, uint> ControllerButtonIds(MachineConfiguration configuration) => new Dictionary<ControllerCommand, uint>
    {
        [ControllerCommand.Up] = InputConstants.JoypadUpId,
        [ControllerCommand.Down] = InputConstants.JoypadDownId,
        [ControllerCommand.Left] = InputConstants.JoypadLeftId,
        [ControllerCommand.Right] = InputConstants.JoypadRightId,
        [ControllerCommand.A] = InputConstants.JoypadBId,
        [ControllerCommand.B] = InputConstants.JoypadAId,
        [ControllerCommand.C] = InputConstants.JoypadYId,
        [ControllerCommand.D] = InputConstants.JoypadXId,
        [ControllerCommand.Start] = InputConstants.JoypadStartId,
        [ControllerCommand.Select] = InputConstants.JoypadSelectId
    };

    protected static IReadOnlyDictionary<ControllerCommand, uint> PocketControllerButtonIds() => new Dictionary<ControllerCommand, uint>
    {
        [ControllerCommand.Up] = InputConstants.JoypadUpId,
        [ControllerCommand.Down] = InputConstants.JoypadDownId,
        [ControllerCommand.Left] = InputConstants.JoypadLeftId,
        [ControllerCommand.Right] = InputConstants.JoypadRightId,
        [ControllerCommand.A] = InputConstants.JoypadBId,
        [ControllerCommand.B] = InputConstants.JoypadAId,
        [ControllerCommand.Option] = InputConstants.JoypadStartId
    };
    internal virtual ulong InputCapabilities(MachineConfiguration configuration) => (1UL << checked((int)InputConstants.JoypadDevice)) | (1UL << checked((int)InputConstants.AnalogDevice));
    internal virtual uint ControllerDeviceId(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => peripheral == PeripheralCategory.None ? InputConstants.NoDevice : InputConstants.JoypadDevice;
    internal abstract void PrepareFirmware(MachineConfiguration configuration, string systemDirectory);
    internal abstract LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions);
    internal virtual bool LoadContent(MachineConfiguration configuration, ExternalCoreExports exports, LoadedContent? content) => exports.LoadGame(content?.GameInfo ?? nint.Zero);
    internal virtual bool SupportsMediaExchange(MachineConfiguration configuration, EmulationMediaSlot slot) => false;
    internal virtual void CompleteMediaLoad(MachineConfiguration configuration, string sessionDirectory, ExternalHostCallbacks callbacks) { }
    internal virtual object? PreparePersistentMedia(MachineConfiguration configuration, string saveDirectory) => null;
    internal virtual object? PreparePersistentMedia(MachineConfiguration configuration, string saveDirectory, LoadedContent? content) => PreparePersistentMedia(configuration, saveDirectory);
    internal virtual void CompletePersistentMedia(object? state) { }

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
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory) .GetActiveInstallationAsync(Emulator, cancellationToken).ConfigureAwait(false);
        var version = installation is null ? null : Path.GetFileName(installation.VersionDirectory);
        return new EmulationEmulatorInstallation(Definition, version);
    }

    public override async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync( EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory) .GetAvailableAsync(Emulator, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, $"{item.DeclaredVersion} · {item.PublishedUtc.LocalDateTime:g}", item.DeclaredVersion)).ToArray();
    }

    public override async ValueTask<string> InstallAsync(EmulatorManagementContext context, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var selected = CoreReleaseFunctions.Releases.SelectRelease(_availableReleases, release, selected => selected.Emulator == Emulator);
        var adapter = CoreReleaseFunctions.Releases.CreateInstallProgress<CoreInstallProgress>(progress, static value => value.Fraction);
        var installation = await new CoreReleaseService(context.HttpClient, context.CoreDirectory) .InstallAsync(selected, adapter, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public override async ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken) => await new CoreProvider(context.HttpClient, context.CoreDirectory) .FindInstalledPathAsync(Emulator, cancellationToken).ConfigureAwait(false);

    protected override IEmulatorCore CreateCore(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        return new ProcessCore(context.HostExecutablePath, context.CorePath, Emulator);
    }
}
