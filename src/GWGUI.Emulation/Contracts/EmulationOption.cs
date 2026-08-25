namespace GWGUI.Emulation.Contracts;

public sealed record EmulationOption(
    string Key,
    string Name,
    string? Description,
    string? Category,
    string DefaultValue,
    string CurrentValue,
    IReadOnlyList<EmulationOptionValue> Values,
    bool IsVisible = true);
