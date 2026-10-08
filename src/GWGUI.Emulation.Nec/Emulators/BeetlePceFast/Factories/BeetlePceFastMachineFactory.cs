using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Services;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;
using System.IO;
using GWGUI.Emulation.Nec.Common.Machines.PcEngine.Constants;
using GWGUI.Emulation.Nec.Common.Machines.CoreGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineLt.Constants;
using GWGUI.Emulation.Nec.Common.Machines.TurboExpress.Constants;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Factories;

internal sealed class BeetlePceFastMachineFactory : IEmulatorAdapter
{
    public EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) =>
        GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreStorageSettingsFunctions.Describe(configuration);
    public MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) =>
        GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreStorageSettingsFunctions.Apply(configuration, settings);

    public IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration) =>
        GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreSettingsDescriptionFunctions.Create(configuration);

    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public string EmulatorId => BeetlePceFastConstants.Id;
    public string EmulatorKey => BeetlePceFastConstants.Id;
    public EmulationEmulatorDefinition Definition { get; } = new(
        BeetlePceFastConstants.Id, BeetlePceFastConstants.DisplayName,
        BeetlePceFastConstants.DescriptionResourceKey,
        new[] { PcEngineMachineConstants.Id, CoreGrafxMachineConstants.Id,
            PcEngineDuoMachineConstants.Id, PcEngineLtMachineConstants.Id,
            TurboExpressMachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineCd.Constants.MachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineSuperCd.Constants.MachineConstants.Id ,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineArcadeCard.Constants.MachineConstants.Id }
            .ToHashSet(StringComparer.Ordinal));

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
        var service = new CoreReleaseService(context.HttpClient, context.CoreDirectory, ReleaseConstants.Settings);
        var version = service.GetInstalledVersion();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(Definition, version));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(context.HttpClient, context.CoreDirectory, ReleaseConstants.Settings)
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
        return await new CoreReleaseService(context.HttpClient, context.CoreDirectory, ReleaseConstants.Settings)
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
        var native = BeetlePceFastOptionFunctions.ToNative(configuration.EnsureId());
        return new Machine(machineId, native,
            new ProcessCore(context.HostExecutablePath, context.CorePath),
            native.Media ?? [], Path.Combine(context.SessionsDirectory,
                machineId.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)),
            native.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration));
    }
}
