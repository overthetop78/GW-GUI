namespace GWGUI.Emulation;

public sealed record EmulationSettingsChoice(
    string Id,
    string DisplayResourceKey,
    string? InvariantDisplayValue = null,
    long? NumericValue = null);
