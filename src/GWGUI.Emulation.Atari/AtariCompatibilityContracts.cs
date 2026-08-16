using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public enum AtariSettingsTab
{
    General,
    Cpu,
    Memory,
    Firmware,
    Video,
    Audio,
    Storage,
    Keyboard,
    Mouse,
    Controllers
}

public enum AtariSettingsGroup
{
    Machine,
    Processor,
    Memory,
    Firmware,
    Display,
    Rendering,
    AudioOutput,
    StorageDevices,
    KeyboardAssignments,
    MouseAssignments,
    ControllerAssignments
}

public enum AtariSettingOption
{
    CpuModel,
    CpuPrecision,
    CpuSpeed,
    Fpu,
    MainMemory,
    AlternateMemory,
    Firmware,
    Region,
    VideoStandard,
    Renderer,
    AudioEnabled,
    Storage,
    KeyboardMappings,
    MouseMappings,
    ControllerMappings
}

public enum AtariOptionAvailability
{
    Editable,
    Forced,
    Unavailable
}

public enum AtariMediaAvailability
{
    Available,
    Unavailable
}

public sealed record AtariOptionRule(
    AtariSettingOption Option,
    AtariOptionAvailability Availability,
    string? ForcedValue = null,
    string? ExplanationResourceKey = null);

public sealed record AtariMediaCompatibilityRule(
    AtariMediaKind Kind,
    IReadOnlyList<EmulationMediaSlot> Slots,
    AtariMediaAvailability Availability = AtariMediaAvailability.Available,
    string? ExplanationResourceKey = null);

public sealed record AtariCompatibilityDefinition(
    AtariMachineModel Model,
    AtariCoreKind Core,
    IReadOnlyList<AtariSettingsTab> VisibleTabs,
    IReadOnlyList<AtariSettingsGroup> VisibleGroups,
    IReadOnlyList<AtariOptionRule> Options,
    IReadOnlyList<AtariFirmwareKind> Firmware,
    IReadOnlyList<AtariMediaCompatibilityRule> Media,
    int ControllerPortCount);
