using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal sealed record AtariHardwareChoice(string Value, string DisplayName);

internal sealed record AtariHardwareField(
    AtariSettingOption Option,
    string ResourceKey,
    IReadOnlyList<AtariHardwareChoice> Choices,
    string SelectedValue,
    AtariOptionAvailability Availability,
    string? ExplanationResourceKey);

internal sealed record AtariHardwareView(
    IReadOnlyList<AtariHardwareField> Cpu,
    IReadOnlyList<AtariHardwareField> Memory,
    IReadOnlyList<AtariFirmwareDefinition> Firmware,
    IReadOnlyList<AtariHardwareChoice> Regions);
