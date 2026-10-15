namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

internal sealed record CoreDefinition(
    string Id,
    string LibraryName,
    string LibraryFileName,
    IReadOnlyList<string> Extensions,
    CoreReleaseSettings ReleaseSettings,
    IReadOnlyList<CoreOption> Options,
    IReadOnlyList<FirmwareSlot> Firmware,
    EmulationEmulatorDefinition Emulator,
    bool RequiresExternalFirmware,
    IReadOnlyList<ControllerType> ControllerTypes,
    TimeSpan? StartupTimeout = null,
    TimeSpan? ShutdownTimeout = null,
    IReadOnlyList<string>? StartupFailureDiagnostics = null,
    bool PersistentDataDirectories = false,
    bool SupportsSavedStates = true,
    IReadOnlyList<ControllerType>? FeedbackControllerTypes = null,
    TimeSpan? DisposalTimeout = null,
    IReadOnlyList<string>? PreviousLibraryNames = null);
