namespace GWGUI.Emulation.Atari;

public sealed record AtariHardwareField(
    AtariSettingOption Option,
    string ResourceKey,
    IReadOnlyList<AtariHardwareChoice> Choices,
    string SelectedValue,
    AtariOptionAvailability Availability,
    string? ExplanationResourceKey);
