namespace GWGUI.Emulation.Amstrad.Common.Interfaces;

internal interface IEmulatorAdapter
{
    IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration,
        IReadOnlyList<CoreOption> coreOptions);
    string EmulatorId { get; }
    string EmulatorKey { get; }
    EmulationEmulatorDefinition Definition { get; }
    IReadOnlyList<CoreOption> GetOptions(EmulatorManagementContext context);
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

