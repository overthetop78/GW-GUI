namespace GWGUI.Emulation.Atari.Common.Interfaces;

internal interface IEmulatorAdapter
{
    string EmulatorId { get; }
    string EmulatorKey { get; }
    EmulationEmulatorDefinition Definition { get; }
    bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode);
    ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken);
    ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken);
    ValueTask<string> InstallAsync(EmulatorManagementContext context,
        EmulationEmulatorRelease release, IProgress<double>? progress,
        CancellationToken cancellationToken);
    ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context,
        CancellationToken cancellationToken);
    IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(MachineConfiguration configuration);
    Machine Create(MachineConfiguration configuration, EmulatorCreationContext context);
}
