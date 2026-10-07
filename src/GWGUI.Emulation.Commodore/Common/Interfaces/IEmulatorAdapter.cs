namespace GWGUI.Emulation.Commodore.Common.Interfaces;

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
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);
    IReadOnlyList<EmulationFirmwareCandidate> ScanFirmware(MachineConfiguration configuration, string directory, CancellationToken cancellationToken);
    ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory);
    Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory);
    IReadOnlyList<EmulationMediaDevice> GetMediaDevices(MachineConfiguration configuration);
    IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates);
    IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(Model model, MachineConfiguration configuration);
    EmulationStorageSettings DescribeStorage(MachineConfiguration configuration);
    MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings);
    MachineConfiguration ApplyBackendSettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values);
    EmulationConfigurationSummary Summarize(MachineConfiguration configuration);
    void ValidateConfiguration(MachineConfiguration configuration);
    IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration);
    int GetControllerPortCount(MachineConfiguration configuration);
    MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration);
    IReadOnlySet<EmulationKey> SoftResetKeys { get; }
    Machine Create(MachineConfiguration configuration, EmulatorCreationContext context);
}
