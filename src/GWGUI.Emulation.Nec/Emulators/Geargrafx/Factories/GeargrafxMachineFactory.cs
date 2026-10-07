using System.IO;
using GWGUI.Emulation.Nec.Common.Machines.CoreGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.LaserActive.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngine.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineLt.Constants;
using GWGUI.Emulation.Nec.Common.Machines.SuperGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.TurboExpress.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Services;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions;

namespace GWGUI.Emulation.Nec.Emulators.Geargrafx.Factories;

internal sealed class GeargrafxMachineFactory : IEmulatorAdapter
{
    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public string EmulatorId => GeargrafxConstants.Id;
    public string EmulatorKey => GeargrafxConstants.Id;
    public EmulationEmulatorDefinition Definition { get; } = new(
        GeargrafxConstants.Id, GeargrafxConstants.DisplayName,
        GeargrafxConstants.DescriptionResourceKey,
        new[] { PcEngineMachineConstants.Id, CoreGrafxMachineConstants.Id,
            PcEngineDuoMachineConstants.Id, PcEngineLtMachineConstants.Id,
            TurboExpressMachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineCd.Constants.MachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineSuperCd.Constants.MachineConstants.Id, SuperGrafxMachineConstants.Id,
            LaserActiveMachineConstants.Id }.ToHashSet(StringComparer.Ordinal));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        return false;
    }

    public ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition,
            ReleaseService(context).GetInstalledVersion()));
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
        var native = BeetlePceFastOptionFunctions.ToNative(configuration.EnsureId());
        var sessionDirectory = Path.Combine(context.SessionsDirectory,
            machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        GeargrafxFirmwareFunctions.Prepare(context.FirmwareDirectory, sessionDirectory,
            configuration.FirmwarePath);
        return new Machine(machineId, native,
            new ProcessCore(context.HostExecutablePath, context.CorePath),
            native.Media ?? [], sessionDirectory,
            native.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration));
    }

    private static CoreReleaseService ReleaseService(EmulatorManagementContext context) =>
        new(context.HttpClient, context.CoreDirectory, GeargrafxConstants.ReleaseSettings);
}
