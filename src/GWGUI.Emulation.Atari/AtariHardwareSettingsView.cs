namespace GWGUI.Emulation.Atari;

public sealed record AtariHardwareChoice(string Value, string DisplayName, long? Bytes = null);

public sealed record AtariHardwareField(
    AtariSettingOption Option,
    string ResourceKey,
    IReadOnlyList<AtariHardwareChoice> Choices,
    string SelectedValue,
    AtariOptionAvailability Availability,
    string? ExplanationResourceKey);

public sealed record AtariHardwareView(
    IReadOnlyList<AtariHardwareField> Cpu,
    IReadOnlyList<AtariHardwareField> Memory,
    IReadOnlyList<AtariFirmwareDefinition> Firmware,
    IReadOnlyList<AtariHardwareChoice> Regions);
