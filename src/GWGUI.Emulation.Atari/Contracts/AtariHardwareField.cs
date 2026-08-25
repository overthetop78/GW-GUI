namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariHardwareField(
    AtariSettingOption Option,
    string ResourceKey,
    IReadOnlyList<AtariHardwareChoice> Choices,
    string SelectedValue,
    AtariOptionAvailability Availability,
    string? ExplanationResourceKey);
