using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;

public sealed record CompatibilityDefinition(
    MachineModel Model,
    Emulator Core,
    IReadOnlyList<SettingsTab> VisibleTabs,
    IReadOnlyList<FirmwareCategory> Firmware,
    IReadOnlyList<MediaCompatibilityRule> Media,
    int ControllerPortCount);

public sealed record MediaCompatibilityRule(
    MediaCategory Category,
    IReadOnlyList<EmulationMediaSlot> Slots,
    MediaAvailability Availability = MediaAvailability.Available,
    string? ExplanationResourceKey = null);

public sealed record MediaConfiguration(
    string Path,
    MediaCategory Category,
    EmulationMediaSlot Slot,
    string? Label = null,
    bool IsReadOnly = false,
    bool IsInserted = true,
    StorageBus? StorageBus = null,
    string? MountPoint = null,
    int MountOrder = MediaConstants.DefaultMountOrder,
    CartridgePlatform? CartridgePlatform = null,
    int? CartridgeType = null,
    bool CassetteAutoBoot = false,
    CartridgeRegion? CartridgeRegion = null);
