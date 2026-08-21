namespace GWGUI.Emulation.Atari;

public sealed record AtariOptionRule(
    AtariSettingOption Option,
    AtariOptionAvailability Availability,
    string? ForcedValue = null,
    string? ExplanationResourceKey = null);
