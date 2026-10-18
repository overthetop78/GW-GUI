namespace GWGUI.Emulation.Contracts;

public sealed record EmulationResourceRestartOption(
    string Key,
    string Name,
    string? Description,
    string? Category,
    string DefaultValue,
    string CurrentValue,
    IReadOnlyList<EmulationResourceOptionValue> Values,
    bool IsVisible = true,
    string? CategorizedName = null,
    string? CategorizedDescription = null,
    bool RequiresRestart = false,
    string? ResourceKey = null,
    string? ExplanationResourceKey = null
);
