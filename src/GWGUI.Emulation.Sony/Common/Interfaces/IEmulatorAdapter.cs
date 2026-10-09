namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorAdapter
{
    bool RequiresExternalFirmware { get; }
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);
    IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration);
    IReadOnlyList<ControllerType> GetControllerTypes(MachineConfiguration configuration);
    IReadOnlyList<ControllerType> GetControllerTypes(MachineConfiguration configuration, int port) =>
        GetControllerTypes(configuration);
    int GetControllerPortCount(MachineConfiguration configuration) =>
        ModelCatalog.Get(configuration.Model).ControllerPortCount;
    IReadOnlyList<string> SupportedContentExtensions { get; }
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

