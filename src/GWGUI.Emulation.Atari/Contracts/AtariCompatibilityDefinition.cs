namespace GWGUI.Emulation.Atari;

public sealed record AtariCompatibilityDefinition(
    AtariMachineModel Model,
    AtariEmulator Core,
    IReadOnlyList<AtariSettingsTab> VisibleTabs,
    IReadOnlyList<AtariSettingsGroup> VisibleGroups,
    IReadOnlyList<AtariOptionRule> Options,
    IReadOnlyList<AtariFirmwareCategory> Firmware,
    IReadOnlyList<AtariMediaCompatibilityRule> Media,
    int ControllerPortCount);
