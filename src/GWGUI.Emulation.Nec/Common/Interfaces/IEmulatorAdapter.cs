namespace GWGUI.Emulation.Nec.Common.Interfaces;

internal interface IEmulatorAdapter
{
    IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration);
    EmulationStorageSettings DescribeStorage(MachineConfiguration configuration);
    MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings);
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

