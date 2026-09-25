using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Contracts;

public sealed record CompatibilityDefinition(
    MachineModel Model,
    Emulator Core,
    IReadOnlyList<SettingsTab> VisibleTabs,
    IReadOnlyList<SettingsGroup> VisibleGroups,
    IReadOnlyList<OptionRule> Options,
    IReadOnlyList<FirmwareCategory> Firmware,
    IReadOnlyList<MediaCompatibilityRule> Media,
    int ControllerPortCount);

internal sealed record DiskImageStatus(int Index, string? Path, string? Label);

internal sealed record DiskStatus(
    int ImageCount,
    int CurrentIndex,
    bool IsEjected,
    IReadOnlyList<DiskImageStatus> Images);

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

internal sealed record PreparedCartridge(
    MediaConfiguration Configuration,
    Emulator Core,
    string RuntimePath,
    bool NeedsFullPath);

internal sealed record SessionMedia(
    MediaConfiguration Configuration,
    string RuntimePath,
    IReadOnlyList<string> SourcePaths,
    IReadOnlyList<string> RuntimePaths,
    bool RequiresExplicitSave);
