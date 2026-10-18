namespace GWGUI.Emulation.Contracts;

public sealed record EmulationTimingOption(
    string Key,
    string Name,
    string? Description,
    string? Category,
    string DefaultValue,
    string CurrentValue,
    IReadOnlyList<EmulationTimingOptionValue> Values,
    bool IsVisible = true,
    string? CategorizedName = null,
    string? CategorizedDescription = null,
    string? ResourceKey = null,
    string? ExplanationResourceKey = null,
    bool RequiresRuntimeReload = false
);
