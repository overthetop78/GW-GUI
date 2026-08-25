namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariOptionRule(
    AtariSettingOption Option,
    AtariOptionAvailability Availability,
    string? ForcedValue = null,
    string? ExplanationResourceKey = null);
